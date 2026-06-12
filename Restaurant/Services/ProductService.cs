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
    private readonly IValidator<ProductCreateRequest> _validator;

    public ProductService(DatabaseContext context, IValidator<ProductCreateRequest> validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<PaginatedDto<ProductDto>> GetAsync(ProductFilter? filters, CancellationToken cancellationToken)
    {
        IQueryable<ProductEntity> query = _context.Products.Where(x => x.IsDeleted == false).AsNoTracking(); // no es necesario mantener el estado de la entidad ya que es solo un GET

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

    public async Task<ProductDto> CreateAsync(ProductCreateRequest request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request,  cancellationToken);

        var entity = new ProductEntity
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Sku = request.Sku,
            IsAvailable = request.IsAvailable,
            CategoryId = request.CategoryId,
            // UserId = // obtained from JWT Claims
        };

        _context.Products.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return ProductMapper.Map(entity);
    }
}
