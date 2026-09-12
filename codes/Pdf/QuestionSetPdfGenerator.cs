using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SoruHavuzu.Domain.Entities;
using SoruHavuzu.Domain.Enums;

namespace SoruHavuzu.Application.Pdf;

public static class QuestionSetPdfGenerator
{
    static QuestionSetPdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private static readonly string[] OptionLabels = ["A", "B", "C", "D"];

    // Problem / açık uçlu sorularda cevap için ayrılan çizgisiz boş yazı alanının yüksekliği.
    private const float OpenEndedAnswerHeightCm = 1f;

    public static byte[] Generate(QuestionSet questionSet)
    {
        return BuildDocument(questionSet).GeneratePdf();
    }

    public static IDocument BuildDocument(QuestionSet questionSet)
    {
        var questions = questionSet.QuestionSetQuestions
            .OrderBy(qsq => qsq.Order)
            .Select(qsq => qsq.Question)
            .ToList();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(1.5f, Unit.Centimetre);
                page.MarginVertical(0.8f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10f).FontFamily("DejaVu Sans"));

                var isFirstPage = true;
                page.Header().Element(c =>
                {
                    ComposeHeader(c, questionSet, isFirstPage);
                    isFirstPage = false;
                });
                page.Content().Element(c => ComposeQuestions(c, questions));
                page.Footer().AlignCenter().Text(x =>
                {
                    x.DefaultTextStyle(s => s.FontSize(8f).FontColor(Colors.Grey.Medium));
                    x.Span("Sayfa ");
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });
    }
    private static void ComposeHeader(IContainer container, QuestionSet qs, bool isFirstPage)
    {
        container.Column(header =>
        {
            header.Spacing(4);

            if (isFirstPage && !string.IsNullOrWhiteSpace(qs.Name))
            {
                header.Item().AlignCenter()
                    .Text(qs.Name)
                    .FontSize(16f)
                    .Bold()
                    .FontColor(Colors.Black);

                header.Item().PaddingTop(4);
            }

            header.Item().Row(info =>
            {
                var fullName = $"{qs.CreatedByUser?.FirstName} {qs.CreatedByUser?.LastName}";
                info.RelativeItem().AlignLeft()
                    .Text($"Hazırlayan: {fullName}")
                    .FontSize(9f).FontColor(Colors.Grey.Darken2);
                info.RelativeItem().AlignRight()
                    .Text($"Tarih: {qs.CreatedAt:dd.MM.yyyy}")
                    .FontSize(9f).FontColor(Colors.Grey.Darken2);
            });

            if (isFirstPage)
            {
                header.Item().PaddingTop(6)
                    .Text("Ad Soyad:").Bold()
                    .FontSize(9f)
                    .FontColor(Colors.Grey.Darken2);
            }

            header.Item().PaddingVertical(3)
                .LineHorizontal(1f)
                .LineColor(Colors.Grey.Lighten2);
        });
    }

    private static void ComposeQuestions(IContainer container, List<Question> questions)
    {
        container.Column(col =>
        {
            col.Item().PaddingTop(6);
            col.Spacing(10);

            for (int i = 0; i < questions.Count; i++)
            {
                var idx = i;
                col.Item().Element(c =>
                    ComposeQuestion(c, questions[idx], idx + 1));
            }
        });
    }

    private static void ComposeQuestion(IContainer container, Question q, int number)
    {
        container.Column(card =>
        {
            card.Spacing(4);

            card.Item().Row(questionRow =>
            {
                questionRow.Spacing(6);
                questionRow.ConstantItem(24)
                    .Text($"{number}.")
                    .FontSize(10.5f).Bold()
                    .FontColor(Colors.Blue.Darken3);
                questionRow.RelativeItem()
                    .Text(CapitalizeFirst(q.QuestionText))
                    .FontSize(10.5f).SemiBold();
            });

            var options = q.Options ?? [];

            switch (q.QuestionType)
            {
                case QuestionType.MultipleChoice:
                    ComposeMultipleChoiceOptions(card, options);
                    break;
                case QuestionType.FillInTheBlank:
                    // Boşluk zaten soru metninin içinde "( .......... )" olarak yer aldığı
                    // için sorunun altına ayrı bir çizgi/cevap alanı çizilmez.
                    break;
                case QuestionType.OpenEnded:
                    ComposeOpenEndedArea(card);
                    break;
                default:
                    ComposeMultipleChoiceOptions(card, options);
                    break;
            }

            card.Item().PaddingTop(6)
                .LineHorizontal(0.8f)
                .LineColor(Colors.Grey.Lighten3);
        });
    }

    private static void ComposeMultipleChoiceOptions(ColumnDescriptor card, List<string> options)
    {
        var count = Math.Min(options.Count, OptionLabels.Length);

        if (count == 0)
        {
            return;
        }

        for (int i = 0; i < count; i += 2)
        {
            card.Item().PaddingLeft(30).Row(optRow =>
            {
                optRow.Spacing(16);

                optRow.RelativeItem().Element(c => ComposeOption(c, options[i], i));

                if (i + 1 < count)
                {
                    optRow.RelativeItem().Element(c => ComposeOption(c, options[i + 1], i + 1));
                }
                else
                {
                    optRow.RelativeItem();
                }
            });
        }
    }

    private static void ComposeOption(IContainer container, string text, int index)
    {
        container.Row(optRow =>
        {
            optRow.Spacing(4);
            optRow.ConstantItem(18)
                .Text($"{OptionLabels[index]})")
                .FontSize(9.5f)
                .FontColor(Colors.Grey.Darken2);
            optRow.RelativeItem()
                .Text(text)
                .FontSize(9.5f)
                .FontColor(Colors.Black);
        });
    }

    private static void ComposeOpenEndedArea(ColumnDescriptor card)
    {
        // Problem / açık uçlu: çizgisiz, yalnızca yazı için sabit dikey boşluk bırakır.
        // Öğrenci işlem basamaklarını serbestçe bu alana yazar.
        card.Item()
            .PaddingLeft(30)
            .Height(OpenEndedAnswerHeightCm, Unit.Centimetre)
            .Text(" ");
    }

    private static string CapitalizeFirst(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return char.ToUpper(text[0]) + text[1..];
    }
}
