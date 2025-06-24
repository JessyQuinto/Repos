using TesorosChoco.Application.DTOs;

namespace TesorosChoco.Application.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    Task<CategoryDto?> GetCategoryByIdAsync(int id);
    Task<CategoryDto?> GetCategoryBySlugAsync(string slug);
}
