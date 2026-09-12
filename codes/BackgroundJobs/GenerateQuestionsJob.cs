using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SoruHavuzu.Application.Interfaces;
using TickerQ.Utilities.Base;

namespace SoruHavuzu.Infrastructure.BackgroundJobs;

/// <summary>Her 5 dakikada bir topics tablosu üzerinden hiyerarşik soru üretimi tetikleyen TickerQ job.</summary>
public sealed class GenerateQuestionsJob(
    IServiceScopeFactory scopeFactory,
    IHostEnvironment environment,
    ILogger<GenerateQuestionsJob> logger)
{
    [TickerFunction("QuestionGeneration.EveryFiveMinutes", "0 */5 * * * *")]
    public async Task EveryFiveMinutesAsync(
        TickerFunctionContext context,
        CancellationToken cancellationToken)
    {
        if (!environment.IsProduction())
        {
            logger.LogDebug(
                "Soru üretim job'ı {Environment} ortamında çalıştırılmadı.",
                environment.EnvironmentName);
            return;
        }

        // Önceki çalıştırma hâlâ devam ediyorsa üst üste binmesin.
        context.CronOccurrenceOperations.SkipIfAlreadyRunning();

        logger.LogInformation(
            "Soru üretim job'ı tetiklendi. JobId: {JobId}, ScheduledFor: {ScheduledFor:O}",
            context.Id, context.ScheduledFor);

        using var scope = scopeFactory.CreateScope();
        var generator = scope.ServiceProvider.GetRequiredService<IQuestionGeneratorService>();
        await generator.ProcessNextBatchAsync(cancellationToken);
    }
}
