using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using CustomerService.Domain.Repositories;
using CustomerService.Domain.ValueObjects;
using SharedKernel.Domain;

namespace CustomerService.Application.Commands;

public record UpdateCustomerAddressCommand(
    Guid CustomerId,
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country) : IRequest<bool>;

public class UpdateCustomerAddressCommandHandler : IRequestHandler<UpdateCustomerAddressCommand, bool>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedCache _cache;

    public UpdateCustomerAddressCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        IDistributedCache cache)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<bool> Handle(UpdateCustomerAddressCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
        if (customer == null)
        {
            throw new DomainException("Customer not found.", "CUSTOMER_NOT_FOUND");
        }

        customer.Address = new Address(
            request.Street,
            request.City,
            request.State,
            request.ZipCode,
            request.Country
        );

        _customerRepository.Update(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate Redis cache
        var cacheKey = $"customer_profile_{request.CustomerId}";
        await _cache.RemoveAsync(cacheKey, cancellationToken);

        return true;
    }
}
