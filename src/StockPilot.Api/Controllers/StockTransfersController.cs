using Microsoft.AspNetCore.Mvc;
using StockPilot.Api.Controllers.Base;
using StockPilot.Api.DTOs.StockTransfers;
using StockPilot.Api.Services.Interfaces;

namespace StockPilot.Api.Controllers;

[ApiController]
[Route("api/companies/{companyId:guid}/stock-transfers")]
public class StockTransfersController : CompanyAuthorizedController
{
    private readonly IStockTransferService _service;

    public StockTransfersController(
        IStockTransferService service,
        IPermissionService permissionService)
        : base(permissionService)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        Guid companyId,
        [FromBody] CreateStockTransferDto dto,
        CancellationToken cancellationToken)
    {
        Guid? assignedWarehouseId = null;

        if (User.HasClaim(c => c.Type == "warehouse_id"))
        {
            if (!TryGetAssignedWarehouseId(out var warehouseId))
                return Forbid();

            assignedWarehouseId = warehouseId;

            if (dto.SourceWarehouseId != warehouseId &&
                dto.DestinationWarehouseId != warehouseId)
            {
                return Forbid();
            }
        }

        var denied = await RequirePermissionAsync(
            companyId,
            "stock.transfer.create",
            assignedWarehouseId,
            cancellationToken);

        if (denied is not null)
            return denied;

        try
        {
            var result = await _service.CreateAsync(companyId, dto);

            return CreatedAtAction(
                nameof(GetById),
                new { companyId, transferId = result.Id },
                result);
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

    [HttpGet]
    public async Task<IActionResult> GetAll(
        Guid companyId,
        CancellationToken cancellationToken)
    {
        Guid? assignedWarehouseId = null;

        if (User.HasClaim(c => c.Type == "warehouse_id"))
        {
            if (!TryGetAssignedWarehouseId(out var warehouseId))
                return Forbid();

            assignedWarehouseId = warehouseId;
        }

        var denied = await RequirePermissionAsync(
            companyId,
            "stock.transfer.view",
            assignedWarehouseId,
            cancellationToken);

        if (denied is not null)
            return denied;

        var result = await _service.GetAllAsync(
            companyId,
            assignedWarehouseId);

        return Ok(result);
    }

    [HttpGet("{transferId:guid}")]
    public async Task<IActionResult> GetById(
        Guid companyId,
        Guid transferId,
        CancellationToken cancellationToken)
    {
        Guid? assignedWarehouseId = null;

        if (User.HasClaim(c => c.Type == "warehouse_id"))
        {
            if (!TryGetAssignedWarehouseId(out var warehouseId))
                return Forbid();

            assignedWarehouseId = warehouseId;
        }

        var denied = await RequirePermissionAsync(
            companyId,
            "stock.transfer.view",
            assignedWarehouseId,
            cancellationToken);

        if (denied is not null)
            return denied;

        try
        {
            var result = await _service.GetByIdAsync(
                companyId,
                transferId,
                assignedWarehouseId);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("{transferId:guid}/post")]
    public async Task<IActionResult> Post(
        Guid companyId,
        Guid transferId,
        CancellationToken cancellationToken)
    {
        var denied = await RequirePermissionAsync(
            companyId,
            "stock.transfer.post",
            cancellationToken: cancellationToken);

        if (denied is not null)
            return denied;

        try
        {
            var result = await _service.PostAsync(
                companyId,
                transferId);

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

    [HttpPost("{transferId:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        Guid companyId,
        Guid transferId,
        CancellationToken cancellationToken)
    {
        var denied = await RequirePermissionAsync(
            companyId,
            "stock.transfer.cancel",
            cancellationToken: cancellationToken);

        if (denied is not null)
            return denied;

        try
        {
            await _service.CancelAsync(companyId, transferId);
            return NoContent();
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
