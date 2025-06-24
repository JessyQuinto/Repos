using System;
using System.Collections.Generic;
using TesorosChoco.Application.DTOs;

namespace TesorosChoco.Application.DTOs.Detailed;

public class ProductDetailDto : ProductDto
{
    public CategoryDto Category { get; set; } = null!;
    public ProducerDto Producer { get; set; } = null!;
    public List<ProductDto> RelatedProducts { get; set; } = new();
    public int ReviewsCount { get; set; }
    public bool IsAvailable { get; set; }
}
