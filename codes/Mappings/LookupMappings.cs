using Domain.Entities;
using SoruHavuzu.Application.DTOs.Lookups;
using SoruHavuzu.Application.DTOs.Topics;
using SoruHavuzu.Domain.Entities;

namespace SoruHavuzu.Application.Mappings;

public static class LookupMappings
{
    public static GradeDto ToDto(this Grade g) => new()
    {
        Id = g.Id,
        Name = g.Name,
        Level = g.Level
    };

    public static LessonDto ToDto(this Lessons l) => new()
    {
        Id = l.Id,
        Name = l.Name
    };

    public static UnitDto ToDto(this Units u) => new()
    {
        Id = u.Id,
        Name = u.Name,
        LessonId = u.LessonId,
        GradeId = u.GradeId
    };

    public static TopicDto ToDto(this Topics t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        LessonId = t.LessonId,
        LessonName = t.Lesson?.Name ?? string.Empty,
        GradeId = t.GradeId,
        UnitId = t.UnitId,
        QuestionCount = t.Questions?.Count ?? 0
    };


    public static SubTopicDto ToDto(this SubTopics t) => new()
    {
        Id = t.Id,
        Name = t.Name,        
        LessonId = t.LessonId,
        GradeId = t.GradeId,
        TopicId = t.TopicId
    };

    public static FilterTopicLessAndGradeResult toTopicFilterDto(this Topics dto) => new(dto.Id, dto.Name);
    public static FilterSubTopicLessAndGradeResult toSubTopicFilterDto(this SubTopics dto) => new(dto.Id, dto.Name);


    public static SchoolDto ToDto(this School s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        CityId = s.CityId,
        CityName = s.City?.Name ?? string.Empty,
        DistrictId = s.DistrictId,
        DistrictName = s.District?.Name ?? string.Empty
    };

    public static CityDto ToDto(this Cities c) => new()
    {
        Id = c.Id,
        CityCode = c.CityCode,
        Name = c.Name
    };

    public static DistrictDto ToDto(this Districts d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        CityId = d.CityId
    };
}
