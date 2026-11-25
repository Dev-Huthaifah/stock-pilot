using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockPilot.Api.Services.Interfaces;

namespace StockPilot.Api.Controllers.Base;

[Authorize]
public abstract class CompanyAuthorizedController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    protected CompanyAuthorizedController(
        IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    protected bool TryGetAssignedWarehouseId(out Guid warehouseId)
    {
        var value = User.FindFirstValue("warehouse_id");
        return Guid.TryParse(value, out warehouseId);
    }

    protected bool TryGetUserId(out Guid userId)
    {
        var value = User.FindFirstValue("sub");
        return Guid.TryParse(value, out userId);
    }

    protected async Task<bool> HasPermissionAsync(
        Guid companyId,
        string permissionCode,
        Guid? warehouseId = null,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
            return false;

        return await _permissionService.HasPermissionAsync(
            userId,
            companyId,
            permissionCode,
            warehouseId,
            cancellationToken);
    }

    protected async Task<IActionResult?> RequirePermissionAsync(
        Guid companyId,
        string permissionCode,
        Guid? warehouseId = null,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out _))
            return Unauthorized();

        var allowed = await HasPermissionAsync(
            companyId,
            permissionCode,
            warehouseId,
            cancellationToken);

        return allowed ? null : Forbid();
    }

    protected async Task<IActionResult?> RequireCompanyPermissionAsync(
        Guid companyId,
        string permissionCode,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var allowed = await _permissionService.HasCompanyPermissionAsync(
            userId,
            companyId,
            permissionCode,
            cancellationToken);

        return allowed ? null : Forbid();
    }
}