using SoruHavuzu.Application.DTOs.Pages;
using SoruHavuzu.Domain.Entities;

namespace SoruHavuzu.Application.Mappings;

/// <summary>
/// Page entity ↔ DTO dönüşümleri
/// </summary>
public static class PageMappings
{
    public static PageDto ToDto(this Page p) => new()
    {
        Id = p.Id,
        Title = p.Title,
        SubTitle = p.SubTitle,
        Content = p.Content,
        ImageUrl = p.ImageUrl,
        CategoryId = p.CategoryId,
        CategoryName = p.Category?.Name ?? string.Empty,
        CreatedAt = p.CreatedAt
    };

    public static PageListDto ToListDto(this Page p) => new()
    {
        Id = p.Id,
        Title = p.Title,
        SubTitle = p.SubTitle,
        ImageUrl = p.ImageUrl,
        CategoryName = p.Category?.Name ?? string.Empty,
        CreatedAt = p.CreatedAt
    };
}