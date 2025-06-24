namespace TesorosChoco.Application.DTOs.Parameters;

public class ProductSearchParameters
{
    public string? Q { get; set; }
    public int? Category { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? Producer { get; set; }
    public bool? Featured { get; set; }
    public int Limit { get; set; } = 20;
    public int Offset { get; set; } = 0;
}

public class PagedResult<T>
{
    public List<T> Products { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
}
