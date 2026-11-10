using Microsoft.AspNetCore.Mvc;
using StockPilot.Api.DTOs.Warehouses;
using StockPilot.Api.Services.Interfaces;

namespace StockPilot.Api.Controllers;

[ApiController]
[Route("api/companies/{companyId:guid}/warehouses")]
public class WarehousesController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;

    public WarehousesController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        Guid companyId,
        [FromBody] CreateWarehouseDto dto)
    {
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
    public async Task<IActionResult> GetAll(Guid companyId)
    {
        var result = await _warehouseService.GetAllAsync(companyId);

        return Ok(result);
    }

    [HttpGet("{warehouseId:guid}")]
    public async Task<IActionResult> GetById(
        Guid companyId,
        Guid warehouseId)
    {
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
        [FromBody] UpdateWarehouseDto dto)
    {
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

    [HttpDelete("{warehouseId:guid}")]
    public async Task<IActionResult> Deactivate(
        Guid companyId,
        Guid warehouseId)
    {
        var result = await _warehouseService.DeactivateAsync(
            companyId,
            warehouseId);

        if (!result)
            return NotFound("Warehouse not found.");

        return NoContent();
    }
}