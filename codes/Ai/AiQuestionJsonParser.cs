using System.Text.Json;
using SoruHavuzu.Application.DTOs.Ai;

namespace SoruHavuzu.Infrastructure.Ai;

/// <summary>
/// Gemini/OpenAI'den dönen model JSON'unu toleranslı şekilde ayrıştırır.
/// Modeller bazen geçersiz/şema dışı JSON üretebilir (sona virgül, dengesiz
/// parantez, Options öğesinin nesne yerine string olması vb.). Bu sınıf mümkün
/// olan durumları kurtarır; kurtaramadığı (gerçekten bozuk) JSON için
/// <see cref="JsonException"/> fırlatır — çağıran taraf retry/graceful karşılar.
/// </summary>
internal static class AiQuestionJsonParser
{
    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip
    };

    public static IReadOnlyList<AiQuestionItemDto> Parse(string? rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
            return [];

        var cleaned = StripCodeFence(rawJson);
        cleaned = ExtractJsonObject(cleaned);
        if (cleaned.Length == 0)
            return [];

        using var doc = JsonDocument.Parse(cleaned, DocumentOptions);

        if (!doc.RootElement.TryGetProperty("Questions", out var questionsEl) ||
            questionsEl.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var result = new List<AiQuestionItemDto>(questionsEl.GetArrayLength());

        foreach (var question in questionsEl.EnumerateArray())
        {
            if (question.ValueKind != JsonValueKind.Object)
                continue;

            var questionText = GetString(question, "QuestionText");
            var questionType = GetString(question, "QuestionType");
            var difficulty = GetString(question, "Difficulty");
            var correctAnswer = GetString(question, "CorrectAnswer");
            var explanation = GetString(question, "Explanation");

            // Eksik/boş soruları atla; geçerli olanları kurtar.
            if (string.IsNullOrWhiteSpace(questionText) || string.IsNullOrWhiteSpace(correctAnswer))
                continue;

            var options = ParseOptions(question);

            result.Add(new AiQuestionItemDto(
                questionText,
                questionType ?? string.Empty,
                options,
                correctAnswer,
                difficulty ?? string.Empty,
                explanation ?? string.Empty));
        }

        return result;
    }

    private static List<AiQuestionOptionDto> ParseOptions(JsonElement question)
    {
        if (!question.TryGetProperty("Options", out var optionsEl) ||
            optionsEl.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var options = new List<AiQuestionOptionDto>(optionsEl.GetArrayLength());
        var index = 0;

        foreach (var option in optionsEl.EnumerateArray())
        {
            switch (option.ValueKind)
            {
                // Beklenen şekil: { "Key": "A", "Text": "6" }
                case JsonValueKind.Object:
                    var key = GetString(option, "Key") ?? DefaultKey(index);
                    var text = GetString(option, "Text");
                    if (!string.IsNullOrWhiteSpace(text))
                        options.Add(new AiQuestionOptionDto(key, text));
                    break;

                // Model bazen sadece string dizi döner: ["A", "B", "C", "D"]
                case JsonValueKind.String:
                    var str = option.GetString();
                    if (!string.IsNullOrWhiteSpace(str))
                        options.Add(new AiQuestionOptionDto(DefaultKey(index), str));
                    break;
            }

            index++;
        }

        return options;
    }

    private static string? GetString(JsonElement obj, string propertyName)
        => obj.TryGetProperty(propertyName, out var el) && el.ValueKind == JsonValueKind.String
            ? el.GetString()
            : null;

    private static string DefaultKey(int index) => index switch
    {
        0 => "A",
        1 => "B",
        2 => "C",
        3 => "D",
        _ => ((char)('A' + index)).ToString()
    };

    private static string StripCodeFence(string raw)
    {
        var text = raw.Trim();
        if (!text.StartsWith("```", StringComparison.Ordinal))
            return text;

        var firstNewline = text.IndexOf('\n');
        if (firstNewline >= 0)
            text = text[(firstNewline + 1)..];

        var closing = text.LastIndexOf("```", StringComparison.Ordinal);
        if (closing >= 0)
            text = text[..closing];

        return text.Trim();
    }

    private static string ExtractJsonObject(string text)
    {
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start < 0 || end <= start)
            return string.Empty;

        return text[start..(end + 1)];
    }
}
