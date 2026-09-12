using SoruHavuzu.Application.DTOs.QuestionSets;
using SoruHavuzu.Domain.Entities;

namespace SoruHavuzu.Application.Mappings;

public static class QuestionSetMappings
{
    public static QuestionSetDto ToDto(this QuestionSet qs, bool includeQuestions = false) => new()
    {
        Id = qs.Id,
        Name = qs.Name,
        Description = qs.Description,
        GradeId = qs.GradeId,
        GradeName = qs.Grade?.Name ?? string.Empty,
        SchoolName = qs.CreatedByUser?.School?.Name,
        CreatedByUserId = qs.CreatedByUserId,
        CreatedByFullName = qs.CreatedByUser is not null
            ? $"{qs.CreatedByUser.FirstName} {qs.CreatedByUser.LastName}"
            : string.Empty,
        QuestionCount = qs.QuestionSetQuestions.Count,
        Status = qs.Status,
        // Order alanına göre sıralanır — öğretmenin belirlediği sıra korunur.
        Questions = includeQuestions
            ? qs.QuestionSetQuestions
                .OrderBy(qsq => qsq.Order)
                .Select(qsq => qsq.Question.ToQuestionDto())
                .ToList()
            : [],
        CreatedAt = qs.CreatedAt
    };

    private static DTOs.Questions.QuestionDto ToQuestionDto(this Question q) =>
        QuestionMappings.ToDto(q);
}
