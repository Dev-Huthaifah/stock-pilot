using Microsoft.AspNetCore.Mvc;
using StockPilot.Api.DTOs.Stock;
using StockPilot.Api.Services.Interfaces;

namespace StockPilot.Api.Controllers;

[ApiController]
[Route("api/companies/{companyId:guid}/stock")]
public class StockController : ControllerBase
{
    private readonly IStockService _stockService;

    public StockController(IStockService stockService)
    {
        _stockService = stockService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        Guid companyId,
        [FromQuery] Guid? warehouseId,
        [FromQuery] Guid? productId)
    {
        var stocks = await _stockService.GetAllAsync(
            companyId,
            warehouseId,
            productId);

        return Ok(stocks);
    }

    [HttpGet("movements")]
    public async Task<IActionResult> GetMovements(
        Guid companyId,
        [FromQuery] Guid? warehouseId,
        [FromQuery] Guid? productId)
    {
        var movements = await _stockService.GetMovementsAsync(
            companyId,
            warehouseId,
            productId);

        return Ok(movements);
    }

    [HttpPost("in")]
    public async Task<IActionResult> StockIn(
        Guid companyId,
        [FromBody] StockInDto dto)
    {
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
        [FromBody] StockOutDto dto)
    {
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
        [FromBody] StockAdjustmentDto dto)
    {
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