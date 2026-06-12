using System.Linq.Expressions;
using Restaurant.Filters;
using Restaurant.Shared.Entities;

namespace Restaurant.Extensions;

public static class ProductQueryExtensions
{
    public static IQueryable<ProductEntity> ApplyFilters(this IQueryable<ProductEntity> query, PaginationFilters? filters)
    {
        if (filters is not ProductFilter f)
            return query;

        return query
            .WhereIf(f.Name is not null, p => p.Name.Contains(f.Name!))
            .WhereIf(f.MinPrice is not null, p => p.Price >= f.MinPrice!.Value)
            .WhereIf(f.MaxPrice is not null, p => p.Price <= f.MaxPrice!.Value)
            .WhereIf(f.IsAvailable is not null, p => p.IsAvailable == f.IsAvailable!.Value)
            .WhereIf(f.CategoryId is not null, p => p.CategoryId == f.CategoryId!.Value);
    }

    private static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate)
        => condition ? query.Where(predicate) : query;
}
