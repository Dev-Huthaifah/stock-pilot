
using Microsoft.AspNetCore.Mvc;
using StockPilot.Api.DTOs.Companies;
using StockPilot.Api.Services.Interfaces;

namespace StockPilot.Api.Controllers;

[ApiController]
[Route("api/platform/companies")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompaniesController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    [HttpPost]
    public async Task<ActionResult<CompanyResponseDto>> Create(
        CreateCompanyDto dto,
        CancellationToken cancellationToken)
    {
        var company = await _companyService.CreateAsync(
            dto,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = company.Id },
            company);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CompanyResponseDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var companies = await _companyService.GetAllAsync(
            cancellationToken);

        return Ok(companies);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CompanyResponseDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var company = await _companyService.GetByIdAsync(
            id,
            cancellationToken);

        if (company is null)
            return NotFound();

        return Ok(company);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CompanyResponseDto>> Update(
        Guid id,
        UpdateCompanyDto dto,
        CancellationToken cancellationToken)
    {
        var company = await _companyService.UpdateAsync(
            id,
            dto,
            cancellationToken);

        if (company is null)
            return NotFound();

        return Ok(company);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _companyService.DeactivateAsync(
            id,
            cancellationToken);

        if (!result)
            return NotFound();

        return NoContent();
    }
}
