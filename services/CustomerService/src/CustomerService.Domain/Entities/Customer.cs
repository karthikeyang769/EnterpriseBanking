using System;
using SharedKernel.Domain;
using CustomerService.Domain.ValueObjects;
using CustomerService.Domain.Enums;

namespace CustomerService.Domain.Entities;

public class Customer : Entity<Guid>
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public Address? Address { get; set; }
    public KycStatus KycStatus { get; set; } = KycStatus.Pending;

    // EF Core constructor
    private Customer() { }

    public Customer(Guid id, string email, string firstName, string lastName, string phoneNumber)
    {
        Id = id;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        KycStatus = KycStatus.Pending;
    }
}
