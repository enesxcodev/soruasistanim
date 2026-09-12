using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SoruHavuzu.Application.DTOs.Ai;
using SoruHavuzu.Application.Interfaces;

namespace SoruHavuzu.Infrastructure.Ai;

/// <summary>Manuel üretim için Gemini varsayılanlı sağlayıcı geçidi. Anahtarlar yalnızca sunucu yapılandırmasından okunur.</summary>
public sealed class ManualQuestionAiProvider(HttpClient httpClient, IConfiguration configuration) : IManualQuestionAiProvider
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IConfiguration _configuration = configuration;

    public Task<IReadOnlyList<AiQuestionItemDto>> GenerateAsync(string provider, string prompt, CancellationToken cancellationToken = default)
        => provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase)
            ? GenerateOpenAiAsync(prompt, cancellationToken)
            : GenerateGeminiAsync(prompt, cancellationToken);

    private async Task<IReadOnlyList<AiQuestionItemDto>> GenerateGeminiAsync(string prompt, CancellationToken cancellationToken)
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Gemini API anahtarı sunucu ayarlarında yapılandırılmamış.");

        var baseUrl = _configuration["Gemini:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta";
        var model = _configuration["Gemini:Model"] ?? "gemini-3.6-flash";
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl.TrimEnd('/')}/models/{model}:generateContent")
        {
            Content = JsonContent.Create(new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } },
                generationConfig = new { responseMimeType = "application/json", temperature = 0.8 }
            })
        };
        request.Headers.TryAddWithoutValidation("x-goog-api-key", apiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(body);
        var text = json.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
        return AiQuestionJsonParser.Parse(text);
    }

    private async Task<IReadOnlyList<AiQuestionItemDto>> GenerateOpenAiAsync(string prompt, CancellationToken cancellationToken)
    {
        // Bilinçli olarak placeholder: anahtar yalnız deployment/user-secrets ortamında verildiğinde çalışır.
        var apiKey = _configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("OpenAI API anahtarı sunucu ayarlarında yapılandırılmamış.");

        var baseUrl = _configuration["OpenAI:BaseUrl"] ?? "https://api.openai.com/v1";
        var model = _configuration["OpenAI:Model"] ?? "gpt-4.1-mini";
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl.TrimEnd('/')}/chat/completions")
        {
            Content = JsonContent.Create(new
            {
                model,
                response_format = new { type = "json_object" },
                messages = new[] { new { role = "user", content = prompt } }
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(body);
        var text = json.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        return AiQuestionJsonParser.Parse(text);
    }
}
