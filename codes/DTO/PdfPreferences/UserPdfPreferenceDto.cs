namespace SoruHavuzu.Application.DTOs.PdfPreferences;

public record UserPdfPreferenceDto
{
    // ── Sayfa Başlığı ──────────────────────────────────────────
    public bool ShowPageTitle { get; init; } = true;
    public string? PageTitleText { get; init; }
    public string PageTitleAlignment { get; init; } = "center";
    public string PageTitleColor { get; init; } = "#1E40AF";
    public int PageTitleFontSize { get; init; } = 14;
    public bool PageTitleBold { get; init; } = true;

    // ── Sınıf / Hazırlayan / Tarih ────────────────────────────
    public bool ShowGrade { get; init; } = true;
    public bool ShowPreparedBy { get; init; } = true;
    public bool ShowDate { get; init; } = true;
    public string MetaAlignment { get; init; } = "space-between";
    public string MetaColor { get; init; } = "#4B5563";
    public int MetaFontSize { get; init; } = 9;

    // ── Soru Seti Başlığı ─────────────────────────────────────
    public string TitleColor { get; init; } = "#1D4ED8";
    public int TitleFontSize { get; init; } = 16;
    public bool TitleBold { get; init; } = true;
    public bool ShowDescription { get; init; } = false;
    public bool ShowQuestionCount { get; init; } = false;
    public bool ShowSetName { get; init; } = false;

    // ── Soru Görünümü ─────────────────────────────────────────
    public string QuestionNumberStyle { get; init; } = "bold";
    public int QuestionFontSize { get; init; } = 10;
    public string OptionLayout { get; init; } = "twoColumn";
    public string CorrectAnswerHighlight { get; init; } = "";
    public bool ShowDifficultyBadge { get; init; } = false;
    public bool ShowLessonTopic { get; init; } = false;

    // ── Sayfa Düzeni ──────────────────────────────────────────
    public string PageSize { get; init; } = "A4";
    public int MarginTop { get; init; } = 6;
    public int MarginBottom { get; init; } = 12;
    public int MarginLeft { get; init; } = 15;
    public int MarginRight { get; init; } = 15;
    public bool ShowPageNumbers { get; init; } = true;
    public bool ShowHeaderLine { get; init; } = true;
    public int LineSpacing { get; init; } = 10;
    public int ProblemAnswerSpacing { get; init; } = 30;
}
