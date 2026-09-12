namespace SoruHavuzu.Application.DTOs.Ai;

public record AiQuestionOptionDto(string Key, string Text);

public record AiQuestionItemDto(
    string QuestionText,
    string QuestionType,       // "MultipleChoice" | "FillInTheBlank" | "OpenEnded"
    List<AiQuestionOptionDto> Options,
    string CorrectAnswer,      // "A" | "B" | "C" | "D"  veya "03.00" veya "32"
    string Difficulty,          // "Kolay" | "Orta" | "Zor"
    string Explanation);        // Çözüm açıklaması (FillInTheBlank / OpenEnded için adım adım)

public record AiQuestionBatchResponse(List<AiQuestionItemDto> Questions);