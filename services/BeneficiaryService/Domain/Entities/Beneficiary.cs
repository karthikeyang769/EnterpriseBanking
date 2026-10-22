using System;

namespace BeneficiaryService.Domain.Entities;

public class Beneficiary
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string BeneficiaryName { get; set; } = string.Empty;
    public string BeneficiaryAccountNumber { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public decimal TransferLimit { get; set; }
}
