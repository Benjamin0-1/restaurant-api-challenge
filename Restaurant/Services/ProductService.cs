using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Restaurant.Dtos;
using Restaurant.Extensions;
using Restaurant.Filters;
using Restaurant.Mappers;
using Restaurant.Requests;
using Restaurant.Services.Interfaces;
using Restaurant.Shared;
using Restaurant.Shared.Entities;

namespace Restaurant.Services;

public class ProductService : IProductService
{
    private readonly DatabaseContext _context;
    private readonly IValidator<ProductCreateRequest> _createValidator;
    private readonly IValidator<ProductUpdateRequest> _updateValidator;

    public ProductService(
        DatabaseContext context,
        IValidator<ProductCreateRequest> createValidator,
        IValidator<ProductUpdateRequest> updateValidator)
    {
        _context = context;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<PaginatedDto<ProductDto>> GetAsync(ProductFilter? filters, 
        int userId, CancellationToken cancellationToken)
    {
        // El usuario solamente puede ver lo que ha creado y no ha eliminado.
        IQueryable<ProductEntity> query = _context.Products.Where(x => x.IsDeleted == false 
            && x.UserId == userId).AsNoTracking(); // no es necesario mantener el estado de la entidad ya que es solo un GET

        query = query.ApplyFilters(filters); // este llama a la QueryExtension

        var totalCount = await query.CountAsync(cancellationToken);
        var pageNumber = filters?.PageNumber ?? 1;
        var pageSize = filters?.PageSize ?? 10;

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedDto<ProductDto>
        {
            Data = items.Select(ProductMapper.Map), // aqui se llama al mapper
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    public async Task<ProductDto> CreateAsync(ProductCreateRequest request, int userId, CancellationToken cancellationToken)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var entity = new ProductEntity
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Sku = request.Sku,
            IsAvailable = request.IsAvailable,
            CategoryId = request.CategoryId,
            UserId = userId
        };

        _context.Products.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return ProductMapper.Map(entity);
    }

    public async Task<ProductDto> UpdateAsync(
        int id,
        ProductUpdateRequest request,
        int userId,
        CancellationToken cancellationToken)
    {
        // esto valida que el usuario solo pueda actualizar lo que le "pertenece"
        var product = await _context.Products
            .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    !p.IsDeleted &&
                    p.UserId == userId,
                cancellationToken);

        if (product is null)
            throw new KeyNotFoundException($"Product {id} not found."); // devolvera NotFound

        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        ApplyUpdates(product, request);

        await _context.SaveChangesAsync(cancellationToken);

        return ProductMapper.Map(product);
    }

    public async Task<bool> DeleteAsync(int id, int userId, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id 
                                                                       && !p.IsDeleted && p.UserId == userId, cancellationToken);

        if (product is null) return false;

        product.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
    
    private static void ApplyUpdates(ProductEntity product, ProductUpdateRequest request)
    {
        if (request.Name is not null)
            product.Name = request.Name;

        if (request.Description is not null)
            product.Description = request.Description;

        if (request.Price is not null)
            product.Price = request.Price.Value;

        if (request.Sku is not null)
            product.Sku = request.Sku;

        if (request.IsAvailable is not null)
            product.IsAvailable = request.IsAvailable.Value;

        if (request.CategoryId is not null)
            product.CategoryId = request.CategoryId.Value;
    }
}
