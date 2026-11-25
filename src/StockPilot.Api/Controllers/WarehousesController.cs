using Microsoft.AspNetCore.Mvc;
using StockPilot.Api.Controllers.Base;
using StockPilot.Api.DTOs.Warehouses;
using StockPilot.Api.Services.Interfaces;

namespace StockPilot.Api.Controllers;

[ApiController]
[Route("api/companies/{companyId:guid}/warehouses")]
public class WarehousesController : CompanyAuthorizedController
{
    private readonly IWarehouseService _warehouseService;

    public WarehousesController(
        IWarehouseService warehouseService,
        IPermissionService permissionService)
        : base(permissionService)
    {
        _warehouseService = warehouseService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        Guid companyId,
        [FromBody] CreateWarehouseDto dto,
        CancellationToken cancellationToken)
    {
        var denied = await RequirePermissionAsync(
            companyId,
            "warehouses.manage",
            cancellationToken: cancellationToken);

        if (denied is not null)
            return denied;

        try
        {
            var result = await _warehouseService.CreateAsync(companyId, dto);

            if (result is null)
                return NotFound("Company not found or inactive.");

            return CreatedAtAction(
                nameof(GetById),
                new { companyId, warehouseId = result.Id },
                result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        Guid companyId,
        CancellationToken cancellationToken)
    {
        if (TryGetAssignedWarehouseId(out var assignedWarehouseId))
        {
            var denied = await RequirePermissionAsync(
                companyId,
                "warehouses.view",
                assignedWarehouseId,
                cancellationToken);

            if (denied is not null)
                return denied;

            var assignedWarehouse = await _warehouseService.GetByIdAsync(
                companyId,
                assignedWarehouseId);

            if (assignedWarehouse is null)
                return NotFound("Warehouse not found.");

            return Ok(new[] { assignedWarehouse });
        }

        var companyDenied = await RequirePermissionAsync(
            companyId,
            "warehouses.view",
            cancellationToken: cancellationToken);

        if (companyDenied is not null)
            return companyDenied;

        var result = await _warehouseService.GetAllAsync(companyId);

        return Ok(result);
    }

    [HttpGet("{warehouseId:guid}")]
    public async Task<IActionResult> GetById(
        Guid companyId,
        Guid warehouseId,
        CancellationToken cancellationToken)
    {
        var denied = await RequirePermissionAsync(
            companyId,
            "warehouses.view",
            warehouseId,
            cancellationToken);

        if (denied is not null)
            return denied;

        var result = await _warehouseService.GetByIdAsync(
            companyId,
            warehouseId);

        if (result is null)
            return NotFound("Warehouse not found.");

        return Ok(result);
    }

    [HttpPut("{warehouseId:guid}")]
    public async Task<IActionResult> Update(
        Guid companyId,
        Guid warehouseId,
        [FromBody] UpdateWarehouseDto dto,
        CancellationToken cancellationToken)
    {
        var denied = await RequirePermissionAsync(
            companyId,
            "warehouses.manage",
            cancellationToken: cancellationToken);

        if (denied is not null)
            return denied;

        try
        {
            var result = await _warehouseService.UpdateAsync(
                companyId,
                warehouseId,
                dto);

            if (result is null)
                return NotFound("Warehouse not found.");

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPatch("{warehouseId:guid}/activate")]
    public async Task<IActionResult> Activate(
        Guid companyId,
        Guid warehouseId,
        CancellationToken cancellationToken)
    {
        var denied = await RequirePermissionAsync(
            companyId,
            "warehouses.manage",
            cancellationToken: cancellationToken);

        if (denied is not null)
            return denied;

        var result = await _warehouseService.ActivateAsync(
            companyId,
            warehouseId);

        if (!result)
            return NotFound("Warehouse not found.");

        return NoContent();
    }

    [HttpDelete("{warehouseId:guid}")]
    public async Task<IActionResult> Deactivate(
        Guid companyId,
        Guid warehouseId,
        CancellationToken cancellationToken)
    {
        var denied = await RequirePermissionAsync(
            companyId,
            "warehouses.manage",
            cancellationToken: cancellationToken);

        if (denied is not null)
            return denied;

        var result = await _warehouseService.DeactivateAsync(
            companyId,
            warehouseId);

        if (!result)
            return NotFound("Warehouse not found.");

        return NoContent();
    }
}
