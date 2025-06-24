using Microsoft.Extensions.Logging;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.Interfaces;
using TesorosChoco.Domain.Interfaces;
using AutoMapper;

namespace TesorosChoco.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        ICategoryRepository categoryRepository,
        IMapper mapper,
        ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        try
        {
            _logger.LogInformation("Getting all categories");

            var categories = await _categoryRepository.GetAllAsync();
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);

            _logger.LogInformation("Retrieved {Count} categories", categoryDtos.Count());
            return categoryDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all categories");
            throw new Exception("An error occurred while getting categories");
        }
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Getting category with ID: {CategoryId}", id);

            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Category with ID {CategoryId} not found", id);
                return null;
            }

            return _mapper.Map<CategoryDto>(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category with ID {CategoryId}", id);
            throw new Exception($"An error occurred while getting category with ID {id}");
        }
    }

    public async Task<CategoryDto?> GetCategoryBySlugAsync(string slug)
    {
        try
        {
            _logger.LogInformation("Getting category with slug: {Slug}", slug);

            var category = await _categoryRepository.GetBySlugAsync(slug);
            if (category == null)
            {
                _logger.LogWarning("Category with slug {Slug} not found", slug);
                return null;
            }

            return _mapper.Map<CategoryDto>(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category with slug {Slug}", slug);
            throw new Exception($"An error occurred while getting category with slug '{slug}'");
        }
    }
}
