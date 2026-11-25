using Microsoft.AspNetCore.Mvc;
using StockPilot.Api.Controllers.Base;
using StockPilot.Api.DTOs.Stock;
using StockPilot.Api.Services.Interfaces;

namespace StockPilot.Api.Controllers;

[ApiController]
[Route("api/companies/{companyId:guid}/stock")]
public class StockController : CompanyAuthorizedController
{
    private readonly IStockService _stockService;

    public StockController(
        IStockService stockService,
        IPermissionService permissionService)
        : base(permissionService)
    {
        _stockService = stockService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        Guid companyId,
        [FromQuery] Guid? warehouseId,
        [FromQuery] Guid? productId,
        CancellationToken cancellationToken)
    {
        Guid? effectiveWarehouseId = warehouseId;

        if (User.HasClaim(c => c.Type == "warehouse_id"))
        {
            if (!TryGetAssignedWarehouseId(out var assignedWarehouseId))
                return Forbid();

            if (warehouseId.HasValue &&
                warehouseId.Value != assignedWarehouseId)
            {
                return Forbid();
            }

            effectiveWarehouseId = assignedWarehouseId;
        }

        var denied = await RequirePermissionAsync(
            companyId,
            "stock.view",
            effectiveWarehouseId,
            cancellationToken);

        if (denied is not null)
            return denied;

        var stocks = await _stockService.GetAllAsync(
            companyId,
            effectiveWarehouseId,
            productId);

        return Ok(stocks);
    }

    [HttpGet("movements")]
    public async Task<IActionResult> GetMovements(
        Guid companyId,
        [FromQuery] Guid? warehouseId,
        [FromQuery] Guid? productId,
        CancellationToken cancellationToken)
    {
        Guid? effectiveWarehouseId = warehouseId;

        if (User.HasClaim(c => c.Type == "warehouse_id"))
        {
            if (!TryGetAssignedWarehouseId(out var assignedWarehouseId))
                return Forbid();

            if (warehouseId.HasValue &&
                warehouseId.Value != assignedWarehouseId)
            {
                return Forbid();
            }

            effectiveWarehouseId = assignedWarehouseId;
        }

        var denied = await RequirePermissionAsync(
            companyId,
            "stock.view",
            effectiveWarehouseId,
            cancellationToken);

        if (denied is not null)
            return denied;

        var movements = await _stockService.GetMovementsAsync(
            companyId,
            effectiveWarehouseId,
            productId);

        return Ok(movements);
    }

    [HttpPost("in")]
    public async Task<IActionResult> StockIn(
        Guid companyId,
        [FromBody] StockInDto dto,
        CancellationToken cancellationToken)
    {
        var denied = await RequirePermissionAsync(
            companyId,
            "stock.in",
            dto.WarehouseId,
            cancellationToken);

        if (denied is not null)
            return denied;

        try
        {
            var result = await _stockService.StockInAsync(companyId, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("out")]
    public async Task<IActionResult> StockOut(
        Guid companyId,
        [FromBody] StockOutDto dto,
        CancellationToken cancellationToken)
    {
        var denied = await RequirePermissionAsync(
            companyId,
            "stock.out",
            dto.WarehouseId,
            cancellationToken);

        if (denied is not null)
            return denied;

        try
        {
            var result = await _stockService.StockOutAsync(companyId, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("adjustment")]
    public async Task<IActionResult> Adjust(
        Guid companyId,
        [FromBody] StockAdjustmentDto dto,
        CancellationToken cancellationToken)
    {
        var denied = await RequirePermissionAsync(
            companyId,
            "stock.adjust",
            dto.WarehouseId,
            cancellationToken);

        if (denied is not null)
            return denied;

        try
        {
            var result = await _stockService.AdjustAsync(companyId, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
