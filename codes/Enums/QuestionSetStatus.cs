namespace SoruHavuzu.Domain.Enums;

/// <summary>
/// Soru setinin yayınlanma durumu.
/// Yeni oluşturulan setler taslak (Draft) olarak başlar;
/// ileride "yayınla" akışı ile Published durumuna geçirilir.
/// </summary>
public enum QuestionSetStatus
{
    Draft = 0,
    Published = 1
}
