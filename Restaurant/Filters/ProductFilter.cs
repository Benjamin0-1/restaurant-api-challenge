namespace Restaurant.Filters;

public class ProductFilter : PaginationFilters
{
    public string? Name { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public bool? IsAvailable { get; set; }

    public int? CategoryId { get; set; }
}
