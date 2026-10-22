using System;
using SharedKernel.Domain;

namespace TransactionService.Domain.Entities;

public class Transaction : Entity<Guid>
{
    public string SourceAccountNumber { get; set; } = string.Empty;
    public string TargetAccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } = string.Empty; // Transfer, Deposit, Withdraw, Payment
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Success"; // Success, Failed

    // EF constructor
    private Transaction() { }

    public Transaction(string sourceAccountNumber, string targetAccountNumber, decimal amount, string transactionType, string referenceNumber, string description, string status)
    {
        Id = Guid.NewGuid();
        SourceAccountNumber = sourceAccountNumber;
        TargetAccountNumber = targetAccountNumber;
        Amount = amount;
        TransactionType = transactionType;
        ReferenceNumber = referenceNumber;
        Description = description;
        Status = status;
        CreatedAt = DateTime.UtcNow;
    }
}
