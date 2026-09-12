using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoruHavuzu.Application.DTOs.Lookups;
using SoruHavuzu.Application.Interfaces;

namespace SoruHavuzu.API.Controllers;

/// <summary>Ünite (Unit) tanımları — lookup listesi (grade + lesson'a göre) ve admin CRUD.</summary>
[Authorize]
public class UnitsController : ApiControllerBase
{
    private readonly IUnitService _unitService;

    public UnitsController(IUnitService unitService)
    {
        _unitService = unitService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? gradeId,
        [FromQuery] int? lessonId,
        CancellationToken cancellationToken)
        => CreateActionResult(await _unitService.GetUnitsAsync(gradeId, lessonId, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        => CreateActionResult(await _unitService.GetByIdAsync(id, cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUnitRequest request, CancellationToken cancellationToken)
        => CreateActionResult(await _unitService.CreateAsync(request, cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUnitRequest request, CancellationToken cancellationToken)
        => CreateActionResult(await _unitService.UpdateAsync(id, request, cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        => CreateActionResult(await _unitService.DeleteAsync(id, cancellationToken));
}
