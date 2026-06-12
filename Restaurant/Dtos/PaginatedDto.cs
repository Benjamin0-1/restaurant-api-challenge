namespace Restaurant.Dtos;

public class PaginatedDto<TData>
{
    public IEnumerable<TData> Data { get; set; } = [];

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public int TotalPages { get; set; }
}
