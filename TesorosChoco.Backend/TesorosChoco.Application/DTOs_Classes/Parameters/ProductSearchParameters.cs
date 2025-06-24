namespace TesorosChoco.Application.DTOs.Parameters;

public class ProductSearchParameters
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? ProducerId { get; set; }
    public bool? Featured { get; set; }
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 10;
    public string SortBy { get; set; } = "name";
    public string SortOrder { get; set; } = "asc";
}

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int Total { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)Total / Limit);
}
