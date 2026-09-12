namespace SoruHavuzu.Application.DTOs.Ai;

/// <summary>Manuel üretim ekranında seçilen müfredat bağlamı.</summary>
public record ManualQuestionGenerationRequest(
    int GradeId,
    int LessonId,
    int UnitId,
    int TopicId,
    string Provider,
    List<ManualQuestionTypeCountDto> QuestionTypes,
    string? Prompt);

public record ManualQuestionTypeCountDto(string QuestionType, int Count);

public record ManualPromptRequest(
    int GradeId,
    int LessonId,
    int UnitId,
    int TopicId,
    List<ManualQuestionTypeCountDto> QuestionTypes);

public record ManualPromptResponseDto(string Prompt);

public record ManualQuestionGenerationResponseDto(
    IReadOnlyList<AiQuestionItemDto> Questions,
    IReadOnlyList<string> ValidationErrors,
    AiQuestionImportDto ImportPayload);

public record ManualCurriculumResponseDto(
    IReadOnlyList<ManualCurriculumItemDto> Grades,
    IReadOnlyList<ManualCurriculumItemDto> Lessons,
    IReadOnlyList<ManualCurriculumItemDto> Units,
    IReadOnlyList<ManualTopicCurriculumItemDto> Topics);

public record ManualCurriculumItemDto(int Id, string Name, int QuestionCount);

public record ManualTopicCurriculumItemDto(
    int Id,
    string Name,
    int QuestionCount,
    int TargetQuestionCount);
