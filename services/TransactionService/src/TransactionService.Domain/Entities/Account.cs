using System;
using SharedKernel.Domain;

namespace TransactionService.Domain.Entities;

public class Account : Entity<Guid>
{
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Status { get; set; } = "Active";

    // EF Constructor
    private Account() { }

    public void Debit(decimal amount)
    {
        if (Status != "Active") 
            throw new DomainException("Source account is not active.", "ACCOUNT_NOT_ACTIVE");
        if (Balance < amount) 
            throw new DomainException("Insufficient balance.", "INSUFFICIENT_BALANCE");
        Balance -= amount;
    }

    public void Credit(decimal amount)
    {
        if (Status != "Active") 
            throw new DomainException("Target account is not active.", "ACCOUNT_NOT_ACTIVE");
        Balance += amount;
    }
}
