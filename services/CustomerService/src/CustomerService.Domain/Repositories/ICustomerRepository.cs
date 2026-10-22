using System;
using System.Threading.Tasks;
using CustomerService.Domain.Entities;

namespace CustomerService.Domain.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id);
    Task AddAsync(Customer customer);
    void Update(Customer customer);
    void Delete(Customer customer);
}
