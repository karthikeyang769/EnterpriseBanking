using FluentValidation;
using TransactionService.Application.Commands;

namespace TransactionService.Application.Validators;

public class TransferMoneyCommandValidator : AbstractValidator<TransferMoneyCommand>
{
    public TransferMoneyCommandValidator()
    {
        RuleFor(x => x.SourceAccountNumber)
            .NotEmpty().WithMessage("Source account number is required.");

        RuleFor(x => x.TargetAccountNumber)
            .NotEmpty().WithMessage("Target account number is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Transfer amount must be greater than zero.");
    }
}
