using FluentValidation;
using TransactionService.Application.Commands;

namespace TransactionService.Application.Validators;

public class WithdrawCommandValidator : AbstractValidator<WithdrawCommand>
{
    public WithdrawCommandValidator()
    {
        RuleFor(x => x.AccountNumber).NotEmpty().WithMessage("Account number is required.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Withdrawal amount must be greater than zero.");
    }
}
