namespace Restaurant.Requests;

public class ProductUpdateRequest
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public string? Sku { get; set; }

    public bool? IsAvailable { get; set; }

    public int? CategoryId { get; set; }
}