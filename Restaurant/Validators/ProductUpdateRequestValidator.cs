using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Restaurant.Requests;
using Restaurant.Shared;

namespace Restaurant.Validators;

public class ProductUpdateRequestValidator : AbstractValidator<ProductUpdateRequest>
{
    public ProductUpdateRequestValidator(DatabaseContext db)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name)
            .MaximumLength(100)
            .When(x => x.Name is not null);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => x.Description is not null);

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .When(x => x.Price is not null);

        RuleFor(x => x.Sku)
            .NotEmpty()
            .MaximumLength(50)
            .MustAsync(async (sku, ct) =>
                !await db.Products.AnyAsync(p => p.Sku == sku, ct))
            .WithMessage("SKU already exists.")
            .When(x => x.Sku is not null);

        RuleFor(x => x.CategoryId)
            .MustAsync((id, ct) =>
                db.Categories.AnyAsync(c => c.Id == id, ct))
            .WithMessage("Category does not exist.")
            .When(x => x.CategoryId is not null);
    }
}