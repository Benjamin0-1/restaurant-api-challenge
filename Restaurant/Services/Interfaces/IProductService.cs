using Restaurant.Dtos;
using Restaurant.Filters;
using Restaurant.Requests;

namespace Restaurant.Services.Interfaces;

public interface IProductService
{
    Task<PaginatedDto<ProductDto>> GetAsync(ProductFilter? filters, CancellationToken cancellationToken);
    
    Task<ProductDto> CreateAsync(ProductCreateRequest productDto, CancellationToken cancellationToken);

}
