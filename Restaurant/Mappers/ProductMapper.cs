using Restaurant.Dtos;
using Restaurant.Shared.Entities;

namespace Restaurant.Mappers;

public static class ProductMapper
{
    public static readonly Func<ProductEntity, ProductDto> Map = p => new ProductDto
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        Sku = p.Sku,
        IsAvailable = p.IsAvailable,
        CategoryId = p.CategoryId,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };
}
