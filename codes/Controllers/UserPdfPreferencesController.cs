using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoruHavuzu.Application.DTOs.PdfPreferences;
using SoruHavuzu.Application.Interfaces;

namespace SoruHavuzu.API.Controllers;

[Authorize]
public class UserPdfPreferencesController : ApiControllerBase
{
    private readonly IPdfPreferenceService _pdfPreferenceService;

    public UserPdfPreferencesController(IPdfPreferenceService pdfPreferenceService)
    {
        _pdfPreferenceService = pdfPreferenceService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => CreateActionResult(await _pdfPreferenceService.GetForCurrentUserAsync(cancellationToken));

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UserPdfPreferenceDto dto, CancellationToken cancellationToken)
        => CreateActionResult(await _pdfPreferenceService.UpdateAsync(dto, cancellationToken));
}
