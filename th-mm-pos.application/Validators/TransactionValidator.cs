using FluentValidation;
using th_mm_pos.application.DTOs;

namespace th_mm_pos.application.Validators;

public class TransactionValidator : AbstractValidator<TransactionDto>
{
    public TransactionValidator()
    {
        RuleFor(t => t.Items)
            .NotEmpty().WithMessage("Transaction must have at least one item");

        RuleFor(t => t.PaymentMethod)
            .IsInEnum().WithMessage("Invalid payment method");

        RuleFor(t => t.Subtotal)
            .GreaterThanOrEqualTo(0).WithMessage("Subtotal cannot be negative");

        RuleFor(t => t.Tax)
            .GreaterThanOrEqualTo(0).WithMessage("Tax cannot be negative");

        RuleFor(t => t.Discount)
            .GreaterThanOrEqualTo(0).WithMessage("Discount cannot be negative");

        RuleFor(t => t.Total)
            .GreaterThan(0).WithMessage("Total must be greater than 0");

        RuleForEach(t => t.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .GreaterThan(0).WithMessage("Invalid product ID");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");

            item.RuleFor(i => i.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative");
        });
    }
}