using System.Threading.Tasks;
using MassTransit;
using SharedEvents;
using CustomerService.Domain.Entities;
using CustomerService.Domain.Repositories;

namespace CustomerService.Application.Consumers;

public class CustomerRegisteredConsumer : IConsumer<CustomerRegisteredEvent>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CustomerRegisteredConsumer(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<CustomerRegisteredEvent> context)
    {
        var message = context.Message;
        
        var existing = await _customerRepository.GetByIdAsync(message.CustomerId);
        if (existing == null)
        {
            var customer = new Customer(
                message.CustomerId,
                message.Email,
                message.FirstName,
                message.LastName,
                message.PhoneNumber
            );

            await _customerRepository.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
