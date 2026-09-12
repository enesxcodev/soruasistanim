using SoruHavuzu.Application.DTOs.Curriculum;
using SoruHavuzu.Domain.Entities;

namespace SoruHavuzu.Application.Mappings;

public static class CurriculumMappings
{
    public static CurriculumScheduleDto ToDto(this CurriculumSchedule s) => new()
    {
        Id = s.Id,
        SchoolWeek = s.SchoolWeek,
        GradeId = s.GradeId,
        GradeName = s.Grade?.Name ?? string.Empty,
        LessonId = s.LessonId,
        LessonName = s.Lesson?.Name ?? string.Empty,
        TopicId = s.TopicId,
        TopicName = s.Topic?.Name ?? string.Empty
    };
}
