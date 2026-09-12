using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoruHavuzu.Application.DTOs.Lookups;
using SoruHavuzu.Application.Interfaces;

namespace SoruHavuzu.API.Controllers;

[Authorize]
public class LessonsController : ApiControllerBase
{
    private readonly IGradeLessonTopicService _gradeLessonTopicService;

    public LessonsController(IGradeLessonTopicService gradeLessonTopicService)
    {
        _gradeLessonTopicService = gradeLessonTopicService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLessons(CancellationToken cancellationToken)
        => CreateActionResult(await _gradeLessonTopicService.GetLessonsAsync(cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateLesson([FromBody] CreateLessonRequest request, CancellationToken cancellationToken)
        => CreateActionResult(await _gradeLessonTopicService.CreateLessonAsync(request, cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateLesson(int id, [FromBody] UpdateLessonRequest request, CancellationToken cancellationToken)
        => CreateActionResult(await _gradeLessonTopicService.UpdateLessonAsync(id, request, cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteLesson(int id, CancellationToken cancellationToken)
        => CreateActionResult(await _gradeLessonTopicService.DeleteLessonAsync(id, cancellationToken));
}
