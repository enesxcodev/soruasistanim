using SoruHavuzu.Application.DTOs.Categories;
using SoruHavuzu.Domain.Entities;

namespace SoruHavuzu.Application.Mappings;

/// <summary>
/// Category entity ↔ DTO dönüşümleri
/// </summary>
public static class CategoryMappings
{
    public static CategoryDto ToDto(this Category c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Icon = c.Icon,
        PageCount = c.Pages.Count,
        CreatedAt = c.CreatedAt
    };

    public static CategoryListDto ToListDto(this Category c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Icon = c.Icon,
        PageCount = c.Pages.Count
    };
}