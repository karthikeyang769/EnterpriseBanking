using FluentValidation;
using TransactionService.Application.Commands;

namespace TransactionService.Application.Validators;

public class DepositCommandValidator : AbstractValidator<DepositCommand>
{
    public DepositCommandValidator()
    {
        RuleFor(x => x.AccountNumber).NotEmpty().WithMessage("Account number is required.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Deposit amount must be greater than zero.");
    }
}
