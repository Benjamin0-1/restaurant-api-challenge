using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Restaurant.Requests;
using Restaurant.Shared;

namespace Restaurant.Validators;

public class ProductCreateValidator : AbstractValidator<ProductCreateRequest>
{
    public ProductCreateValidator(DatabaseContext db)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name)
            .NotEmpty();

        RuleFor(x => x.Description)
            .NotEmpty();

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.Sku)
            .NotEmpty()
            .MustAsync(async (sku, ct) =>
                !await db.Products.AnyAsync(p => p.Sku == sku, ct))
            .WithMessage("SKU already exists.");

        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .MustAsync((id, ct) =>
                db.Categories.AnyAsync(c => c.Id == id, ct))
            .WithMessage("Category does not exist.");
    }
}
