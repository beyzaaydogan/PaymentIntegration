using FluentValidation;
using PaymentIntegration.API.Models;

namespace PaymentIntegration.Application.Validators;

public class CreatePreOrderRequestValidator : AbstractValidator<CreatePreOrderRequest>
{
    public CreatePreOrderRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId can not be empty!");
        
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount should be greater than 0.");
    }
}