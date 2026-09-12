using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoruHavuzu.Application.DTOs.Users;
using SoruHavuzu.Application.Interfaces;

namespace SoruHavuzu.API.Controllers;

/// <summary>Admin kullanıcı yönetimi REST endpoint'leri.</summary>
[Authorize(Roles = "Admin")]
public class UsersController : ApiControllerBase
{
    private readonly IUserAdminService _userAdminService;

    public UsersController(IUserAdminService userAdminService)
    {
        _userAdminService = userAdminService;
    }

    /// <summary>Tüm kullanıcıları listeler.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => CreateActionResult(await _userAdminService.GetAllAsync(cancellationToken));

    /// <summary>Id ile kullanıcı getirir.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        => CreateActionResult(await _userAdminService.GetByIdAsync(id, cancellationToken));

    /// <summary>Yeni kullanıcı oluşturur.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAdminUserRequest request, CancellationToken cancellationToken)
        => CreateActionResult(await _userAdminService.CreateAsync(request, cancellationToken));

    /// <summary>Kullanıcı bilgilerini günceller.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAdminUserRequest request, CancellationToken cancellationToken)
        => CreateActionResult(await _userAdminService.UpdateAsync(id, request, cancellationToken));

    /// <summary>Kullanıcıyı siler.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        => CreateActionResult(await _userAdminService.DeleteAsync(id, cancellationToken));
}
