using System;

namespace TransactionService.Application.DTOs;

public class TransactionDto
{
    public Guid Id { get; set; }
    public string SourceAccountNumber { get; set; } = string.Empty;
    public string TargetAccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
