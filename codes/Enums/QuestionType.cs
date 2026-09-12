namespace SoruHavuzu.Domain.Enums;

public enum QuestionType
{
    MultipleChoice = 1, // Çoktan Seçmeli (4 Şık)
    FillInTheBlank = 2, // Boşluk Doldurma (Options null/boş, metin içinde '( .......... )')
    OpenEnded = 3       // Problem / Açık Uçlu (Options null/boş, çözüm alanı)
}