
using Microsoft.EntityFrameworkCore;
using StockPilot.Api.Data;
using StockPilot.Api.DTOs.Companies;
using StockPilot.Api.Models;
using StockPilot.Api.Services.Interfaces;

namespace StockPilot.Api.Services.Implementations;

public class CompanyService : ICompanyService
{
    private readonly StockPilotDbContext _context;

    public CompanyService(StockPilotDbContext context)
    {
        _context = context;
    }

    public async Task<CompanyResponseDto> CreateAsync(
        CreateCompanyDto dto,
        CancellationToken cancellationToken = default)
    {
        var company = new Company
        {
            Name = dto.Name.Trim(),
            Email = dto.Email?.Trim(),
            Phone = dto.Phone?.Trim()
        };

        _context.Companies.Add(company);

        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponse(company);
    }

    public async Task<CompanyResponseDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var company = await _context.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        return company is null ? null : MapToResponse(company);
    }

    public async Task<IReadOnlyList<CompanyResponseDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var companies = await _context.Companies
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return companies.Select(MapToResponse).ToList();
    }

    public async Task<CompanyResponseDto?> UpdateAsync(
        Guid id,
        UpdateCompanyDto dto,
        CancellationToken cancellationToken = default)
    {
        var company = await _context.Companies
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (company is null)
            return null;

        company.Name = dto.Name.Trim();
        company.Email = dto.Email?.Trim();
        company.Phone = dto.Phone?.Trim();

        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponse(company);
    }

    public async Task<bool> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var company = await _context.Companies
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (company is null)
            return false;

        company.IsActive = false;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static CompanyResponseDto MapToResponse(Company company)
    {
        return new CompanyResponseDto
        {
            Id = company.Id,
            Name = company.Name,
            Email = company.Email,
            Phone = company.Phone,
            IsActive = company.IsActive,
            CreatedAt = company.CreatedAt
        };
    }
}
