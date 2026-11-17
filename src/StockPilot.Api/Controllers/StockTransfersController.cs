using Microsoft.AspNetCore.Mvc;
using StockPilot.Api.DTOs.StockTransfers;
using StockPilot.Api.Services.Interfaces;

namespace StockPilot.Api.Controllers;

[ApiController]
[Route("api/companies/{companyId:guid}/stock-transfers")]
public class StockTransfersController : ControllerBase
{
    private readonly IStockTransferService _service;

    public StockTransfersController(IStockTransferService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        Guid companyId,
        [FromBody] CreateStockTransferDto dto)
    {
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
    public async Task<IActionResult> GetAll(Guid companyId)
    {
        var result = await _service.GetAllAsync(companyId);
        return Ok(result);
    }

    [HttpGet("{transferId:guid}")]
    public async Task<IActionResult> GetById(
        Guid companyId,
        Guid transferId)
    {
        try
        {
            var result = await _service.GetByIdAsync(
                companyId,
                transferId);

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
        Guid transferId)
    {
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
        Guid transferId)
    {
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