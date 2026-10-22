using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using CustomerService.Domain.Repositories;
using CustomerService.Application.DTOs;
using SharedKernel.Domain;

namespace CustomerService.Application.Queries;

public record GetCustomerByIdQuery(Guid CustomerId) : IRequest<CustomerDto>;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IDistributedCache _cache;

    public GetCustomerByIdQueryHandler(ICustomerRepository customerRepository, IDistributedCache cache)
    {
        _customerRepository = customerRepository;
        _cache = cache;
    }

    public async Task<CustomerDto> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"customer_profile_{request.CustomerId}";
        
        // Check Redis cache first
        var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedData))
        {
            var cachedDto = JsonSerializer.Deserialize<CustomerDto>(cachedData);
            if (cachedDto != null) return cachedDto;
        }

        // Fetch from Postgres if cache miss
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
        if (customer == null)
        {
            throw new DomainException("Customer profile not found.", "CUSTOMER_NOT_FOUND");
        }

        var dto = new CustomerDto
        {
            Id = customer.Id,
            Email = customer.Email,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            PhoneNumber = customer.PhoneNumber,
            Street = customer.Address?.Street ?? string.Empty,
            City = customer.Address?.City ?? string.Empty,
            State = customer.Address?.State ?? string.Empty,
            ZipCode = customer.Address?.ZipCode ?? string.Empty,
            Country = customer.Address?.Country ?? string.Empty,
            KycStatus = customer.KycStatus.ToString()
        };

        // Cache in Redis for 30 minutes
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        };
        var serializedData = JsonSerializer.Serialize(dto);
        await _cache.SetStringAsync(cacheKey, serializedData, cacheOptions, cancellationToken);

        return dto;
    }
}
