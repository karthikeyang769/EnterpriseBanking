using System;
using SharedKernel.Domain;

namespace AccountService.Domain.Entities;

public class Account : Entity<Guid>
{
    public Guid CustomerId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountType { get; set; } = "Savings";
    public decimal Balance { get; set; }
    public string Status { get; set; } = "Active";

    // EF Constructor
    private Account() { }

    public Account(Guid customerId, string accountNumber, string accountType, decimal initialBalance)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        AccountNumber = accountNumber;
        AccountType = accountType;
        Balance = initialBalance;
        Status = "Active";
    }

    public void Credit(decimal amount)
    {
        if (Status != "Active") 
            throw new DomainException("Account is not active.", "ACCOUNT_NOT_ACTIVE");
        Balance += amount;
    }

    public void Debit(decimal amount)
    {
        if (Status != "Active") 
            throw new DomainException("Account is not active.", "ACCOUNT_NOT_ACTIVE");
        if (Balance < amount) 
            throw new DomainException("Insufficient balance.", "INSUFFICIENT_BALANCE");
        Balance -= amount;
    }

    public void SetStatus(string status)
    {
        Status = status;
    }
}
