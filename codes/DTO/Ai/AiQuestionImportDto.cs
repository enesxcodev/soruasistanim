namespace SoruHavuzu.Application.DTOs.Ai;

/// <summary>
/// Chatbot (ChatGPT/Gemini) çıktısından elle üretilen soruları içe aktarmak için zarf DTO.
/// Grade/Lesson/Unit/Topic alanları ID değil İSİM içerir; import servisi bunları veritabanında çözümler.
/// Questions, mevcut AiQuestionItemDto şemasının birebir aynısını kullanır.
/// </summary>
public record AiQuestionImportDto(
    string Grade,
    string Lesson,
    string? Unit,
    string Topic,
    List<AiQuestionItemDto> Questions);

/// <summary>İçe aktarma sonucunun özeti.</summary>
public record QuestionImportReportDto(
    int Added,
    int SkippedDuplicate,
    int Failed,
    IReadOnlyList<string> Errors);
