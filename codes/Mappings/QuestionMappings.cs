using SoruHavuzu.Application.DTOs.Questions;
using SoruHavuzu.Domain.Entities;

namespace SoruHavuzu.Application.Mappings;

public static class QuestionMappings
{
    public static QuestionDto ToDto(this Question q) => new()
    {
        Id = q.Id,
        QuestionText = q.QuestionText,
        QuestionType = q.QuestionType,
        Options = q.Options,
        CorrectAnswer = q.CorrectAnswer,
        Difficulty = q.Difficulty,
        QualityPreference = q.QualityPreference,
        Explanation = q.Explanation,
        GradeId = q.GradeId,
        GradeName = q.Grade?.Name ?? string.Empty,
        LessonId = q.LessonId,
        LessonName = q.Lesson?.Name ?? string.Empty,
        TopicId = q.TopicId,
        TopicName = q.Topic?.Name ?? string.Empty,
        SubTopicId = q.SubTopicId,
        SubTopicName = q.SubTopic?.Name,
        CreatedAt = q.CreatedAt,
        UpdatedAt = q.UpdatedAt
    };
}
