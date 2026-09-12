using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SoruHavuzu.Application.DTOs.Ai;
using SoruHavuzu.Application.Interfaces;

namespace SoruHavuzu.Infrastructure.Ai;

/// <summary>Gemini API ile HTTP üzerinden haberleşen izole servis (Structured Output, System.Text.Json).</summary>
public class GeminiApiService : IGeminiApiService
{
    private const int MaxAttemptsPerModel = 2;

    private static readonly JsonDocumentOptions TolerantJson = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip
    };

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiApiService> _logger;

    public GeminiApiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiApiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AiQuestionItemDto>> GenerateQuestionsAsync(
        string grade, string lesson, string unit, string topic,
        IReadOnlyList<string> existingQuestions, int count = 12, CancellationToken ct = default)
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        var baseUrl = _configuration["Gemini:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta";

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Gemini:ApiKey yapılandırılmamış.");

        var models = ResolveModels();
        var prompt = BuildPrompt(grade, lesson, unit, topic, count, existingQuestions);

        var payload = new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig = new { responseMimeType = "application/json", temperature = 0.8 }
        };

        Exception? lastFailure = null;

        foreach (var model in models)
        {
            var endpoint = $"{baseUrl.TrimEnd('/')}/models/{model}:generateContent";

            for (var attempt = 1; attempt <= MaxAttemptsPerModel; attempt++)
            {
                try
                {
                    using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
                    {
                        Content = JsonContent.Create(payload)
                    };
                    request.Headers.TryAddWithoutValidation("x-goog-api-key", apiKey);

                    var response = await _httpClient.SendAsync(request, ct);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync(ct);
                        return ParseResponse(jsonResponse);
                    }

                    var status = (int)response.StatusCode;
                    var errorBody = await response.Content.ReadAsStringAsync(ct);

                    if (!IsTransientStatus(status))
                        throw new HttpRequestException($"Gemini API Hatası ({status}): {errorBody}");

                    lastFailure = new HttpRequestException($"Gemini API Hatası ({status}): {errorBody}");
                    _logger.LogWarning("Gemini geçici hata. Model={Model}, Status={Status}, Attempt={Attempt}", model, status, attempt);

                    if (attempt < MaxAttemptsPerModel)
                        await Task.Delay(GetRetryDelay(attempt, response), ct);
                }
                catch (OperationCanceledException) when (!ct.IsCancellationRequested)
                {
                    lastFailure = new TimeoutException($"Gemini isteği zaman aşımına uğradı (model: {model}).");
                    _logger.LogWarning(lastFailure, "Gemini zaman aşımı. Model={Model}", model);
                    break;
                }
                catch (JsonException ex)
                {
                    // Model geçersiz/bozuk JSON döndürdü — deterministik değil, tekrar dene.
                    lastFailure = ex;
                    _logger.LogWarning(ex, "Gemini geçersiz JSON yanıtı. Model={Model}, Attempt={Attempt}", model, attempt);

                    if (attempt < MaxAttemptsPerModel)
                        await Task.Delay(GetRetryDelay(attempt), ct);
                }
            }
        }

        throw lastFailure ?? new HttpRequestException("Gemini API çağrısı başarısız oldu.");
    }

    private IReadOnlyList<string> ResolveModels()
    {
        var primary = _configuration["Gemini:Model"] ?? "gemini-3.6-flash";
        var fallbacks = _configuration.GetSection("Gemini:FallbackModels").GetChildren()
            .Select(s => s.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v!.Trim())
            .ToArray();

        var models = new List<string> { primary.Trim() };
        models.AddRange(fallbacks);
        return models.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private IReadOnlyList<AiQuestionItemDto> ParseResponse(string jsonResponse)
    {
        using var doc = JsonDocument.Parse(jsonResponse, TolerantJson);

        if (!doc.RootElement.TryGetProperty("candidates", out var candidatesEl) ||
            candidatesEl.ValueKind != JsonValueKind.Array ||
            candidatesEl.GetArrayLength() == 0)
        {
            _logger.LogWarning("Gemini yanıtı boş/geçersiz candidates dizisi döndü.");
            return [];
        }

        var firstCandidate = candidatesEl[0];
        if (!firstCandidate.TryGetProperty("content", out var contentEl) ||
            !contentEl.TryGetProperty("parts", out var partsEl) ||
            partsEl.ValueKind != JsonValueKind.Array ||
            partsEl.GetArrayLength() == 0 ||
            !partsEl[0].TryGetProperty("text", out var textEl))
        {
            _logger.LogWarning("Gemini yanıtında beklenen content.parts[0].text yapısı bulunamadı.");
            return [];
        }

        var rawJson = textEl.GetString();
        if (string.IsNullOrWhiteSpace(rawJson))
        {
            _logger.LogWarning("Gemini yanıtındaki text alanı boş.");
            return [];
        }

        // Bozuk/şema dışı JSON'a toleranslı ayrıştırma. Kurtarılamayan JSON,
        // retry döngüsü tarafından JsonException olarak yakalanır.
        var result = AiQuestionJsonParser.Parse(rawJson);
        _logger.LogInformation("Gemini yanıtı alındı. Soru sayısı: {Count}", result.Count);
        return result;
    }

    private static bool IsTransientStatus(int status) => status == 429 || status >= 500;

    private static TimeSpan GetRetryDelay(int attempt, HttpResponseMessage? response = null)
    {
        // 429 Retry-After değerini tercih et (işi uzun süre bloke etmemek için 15 sn üst sınır).
        if (response?.Headers.RetryAfter?.Delta is { } retryAfter)
            return retryAfter > TimeSpan.FromSeconds(15) ? TimeSpan.FromSeconds(15) : retryAfter;

        // Üstel geri çekilme + jitter: 1s, 2s, 4s (maks 4s) + 0..500ms.
        var backoffSeconds = Math.Min(4, 1 << (attempt - 1));
        return TimeSpan.FromSeconds(backoffSeconds) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 500));
    }

    private static string BuildPrompt(
        string grade, string lesson, string unit, string topic,
        int count, IReadOnlyList<string> existingQuestions)
    {
        var repeatBlock = existingQuestions is { Count: > 0 }
            ? "\nÖNEMLİ: Aşağıdaki soru kökleriyle AYNI veya ÇOK BENZER olmayan, farklı senaryolara sahip özgün sorular üret:\n" +
              string.Join("\n", existingQuestions.Select((q, i) => $"- {q}")) + "\n"
            : string.Empty;

        return $@"Sen Milli Eğitim Bakanlığı (MEB) müfredatına tam hakim, deneyimli bir ilkokul öğretmenisin.
Hedef Kitle: {grade} düzeyindeki 8-10 yaş grubu çocuklar.

Ders: {lesson}
Ünite: {unit}
Konu: {topic}

GÖREV:
Bu konuya ve sınıf düzeyine uygun TAM {count} adet soru üret.

DİL VE PEDAGOJİ KURALLARI:
1. Dil son derece yalın, açık, sevimli ve Türkçe dilbilgisi kurallarına kusursuz uygun olmalıdır.
2. 3. ve 4. sınıf seviyesine uygun olarak günlük yaşamdan somut örnekler (okul, aile, oyun, doğa, meyveler, sevimli hayvanlar) kullan.
3. Ağır akademik terimlerden ve karmaşık soyut ifadelerden kesinlikle kaçın.

SORU DAĞILIMI (Zorluk ve Tür):
- Zorluk Dağılımı: Kolay, Orta ve Zor seviyeler dengeli dağılsın.
- Soru Türleri Dağılımı (Karma üret):
  1. 'MultipleChoice' (Çoktan Seçmeli): A, B, C, D olmak üzere 4 şıklı ve tek doğru cevaplı.
  2. 'FillInTheBlank' (Boşluk Doldurma): Cümle içinde doldurulacak yeri kesinlikle '( .......... )' şeklinde 10 noktalı parantez ile belirt. 'Options' dizisi BOŞ ([]) olsun. 'CorrectAnswer' alanına sadece boşluğa gelecek kelimeyi/sayıyı yaz.
  3. 'OpenEnded' (Problem / Açık Uçlu Çözüm): Öğrencinin işlem basamaklarını yaparak cevabı bulacağı problem sorusu. 'Options' dizisi BOŞ ([]) olsun. 'CorrectAnswer' alanına nihai sonucu, 'Explanation' alanına adım adım çözümü yaz.

{repeatBlock}

ÇIKTI FORMATI:
SADECE aşağıdaki JSON şemasına uygun geçerli bir JSON dön, markdown veya açıklama ekleme:
{{
  ""Questions"": [
    {{
      ""QuestionText"": ""Görseldeki saatin akrebi 3'ü, yelkovanı 12'yi gösterdiğinde saat ( .......... ) olur."",
      ""QuestionType"": ""FillInTheBlank"",
      ""Difficulty"": ""Kolay"",
      ""Options"": [],
      ""CorrectAnswer"": ""03.00"",
      ""Explanation"": ""Akrep saati, yelkovan dakikayı gösterir.""
    }},
    {{
      ""QuestionText"": ""Ayşe tanesi 4 TL olan kalemlerden 3 tane aldı. Satıcıya 20 TL veren Ayşe kaç TL para üstü alır?"",
      ""QuestionType"": ""MultipleChoice"",
      ""Difficulty"": ""Orta"",
      ""Options"": [
        {{ ""Key"": ""A"", ""Text"": ""6"" }},
        {{ ""Key"": ""B"", ""Text"": ""8"" }},
        {{ ""Key"": ""C"", ""Text"": ""10"" }},
        {{ ""Key"": ""D"", ""Text"": ""12"" }}
      ],
      ""CorrectAnswer"": ""B"",
      ""Explanation"": ""3 x 4 = 12 TL harcadı. 20 - 12 = 8 TL para üstü alır.""
    }},
    {{
      ""QuestionText"": ""Bir çiftlikte 5 inek ve 6 tavuk vardır. Çiftlikteki hayvanların toplam ayak sayısını işlem basamaklarıyla bulunuz."",
      ""QuestionType"": ""OpenEnded"",
      ""Difficulty"": ""Zor"",
      ""Options"": [],
      ""CorrectAnswer"": ""32"",
      ""Explanation"": ""İnek ayakları: 5 x 4 = 20. Tavuk ayakları: 6 x 2 = 12. Toplam: 20 + 12 = 32 ayak.""
    }}
  ]
}}";
    }
}