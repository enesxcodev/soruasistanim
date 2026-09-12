using SoruHavuzu.Application.DTOs.Contact;
using SoruHavuzu.Domain.Entities;

namespace SoruHavuzu.Application.Mappings;

/// <summary>
/// ContactMessage entity ↔ DTO dönüşümleri
/// </summary>
public static class ContactMappings
{
    public static ContactMessageDto ToDto(this ContactMessage m) => new()
    {
        Id = m.Id,
        Name = m.Name,
        Email = m.Email,
        Subject = m.Subject,
        Message = m.Message,
        UserAgent = m.UserAgent,
        IpAddress = m.IpAddress,
        CreatedAt = m.CreatedAt
    };
}
