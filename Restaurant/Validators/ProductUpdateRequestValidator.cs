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

        RuleFor(x => x)
            .CustomAsync(async (_, ctx, ct) =>
            {
                if (!ctx.RootContextData.TryGetValue("productId", out var raw) || raw is not int id)
                    return;

                var exists = await db.Products.AnyAsync(p => p.Id == id && !p.IsDeleted, ct);
                if (!exists)
                    ctx.AddFailure("Id", "Product not found.");
            });

        RuleFor(x => x.Name).NotEmpty().When(x => x.Name is not null);
        RuleFor(x => x.Description).NotEmpty().When(x => x.Description is not null);
        RuleFor(x => x.Price).GreaterThan(0).When(x => x.Price is not null);

        When(x => x.Sku is not null, () =>
        {
            RuleFor(x => x.Sku).NotEmpty();

            RuleFor(x => x.Sku)
                .CustomAsync(async (sku, ctx, ct) =>
                {
                    if (string.IsNullOrEmpty(sku)) return;

                    ctx.RootContextData.TryGetValue("productId", out var raw);
                    var currentId = raw is int id ? id : 0;

                    var isDuplicate = await db.Products.AnyAsync(p => p.Sku == sku && p.Id != currentId, ct);
                    if (isDuplicate)
                        ctx.AddFailure("Sku", "SKU already exists.");
                });
        });

        RuleFor(x => x.CategoryId)
            .MustAsync((catId, ct) => db.Categories.AnyAsync(c => c.Id == catId, ct))
            .WithMessage("Category does not exist.")
            .When(x => x.CategoryId is not null);
    }
}