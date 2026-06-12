using Restaurant.Dtos;
using Restaurant.Filters;
using Restaurant.Requests;

namespace Restaurant.Services.Interfaces;

public interface IProductService
{
    Task<PaginatedDto<ProductDto>> GetAsync(ProductFilter? filters, int userId, CancellationToken cancellationToken);
    
    Task<ProductDto> CreateAsync(ProductCreateRequest productDto, int userId, CancellationToken cancellationToken);

    Task<ProductDto> UpdateAsync(int id, ProductUpdateRequest request, int userId, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(int id, int userId, CancellationToken cancellationToken);
}
