using FluentValidation;
using CustomerService.Application.Commands;

namespace CustomerService.Application.Validators;

public class UpdateCustomerAddressCommandValidator : AbstractValidator<UpdateCustomerAddressCommand>
{
    public UpdateCustomerAddressCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty().WithMessage("Customer ID is required.");
        RuleFor(x => x.Street).NotEmpty().WithMessage("Street is required.").MaximumLength(200);
        RuleFor(x => x.City).NotEmpty().WithMessage("City is required.").MaximumLength(100);
        RuleFor(x => x.State).NotEmpty().WithMessage("State is required.").MaximumLength(100);
        RuleFor(x => x.ZipCode).NotEmpty().WithMessage("Zip code is required.").MaximumLength(20);
        RuleFor(x => x.Country).NotEmpty().WithMessage("Country is required.").MaximumLength(100);
    }
}
