using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockPilot.Api.DTOs.Platform;
using StockPilot.Api.Services.Interfaces;

namespace StockPilot.Api.Controllers;

[ApiController]
[Route("api/platform/company-provisioning")]
[Authorize(Policy = "PlatformAdminOnly")]
public sealed class PlatformCompaniesController : ControllerBase
{
    private readonly IPlatformCompanyService _companyService;

    public PlatformCompaniesController(
        IPlatformCompanyService companyService)
    {
        _companyService = companyService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCompany(
        [FromBody] CreateCompanyRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _companyService.CreateCompanyAsync(
                request,
                cancellationToken);

            return Created(
                $"/api/platform/companies/{result.CompanyId}",
                result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
    [HttpPost("{companyId:guid}/admin")]
    public async Task<IActionResult> CreateCompanyAdmin(
        Guid companyId,
        [FromBody] CreateCompanyAdminRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _companyService.CreateCompanyAdminAsync(
                companyId,
                request,
                cancellationToken);

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