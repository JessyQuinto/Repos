namespace TesorosChoco.Application.DTOs.Parameters;

public class PaginationParameters
{
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 10;
    public string SortBy { get; set; } = "id";
    public string SortOrder { get; set; } = "asc";
    
    public int Offset => (Page - 1) * Limit;
}
