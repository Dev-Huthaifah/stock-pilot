
using StockPilot.Api.DTOs.Companies;

namespace StockPilot.Api.Services.Interfaces;

public interface ICompanyService
{
    Task<CompanyResponseDto> CreateAsync(
        CreateCompanyDto dto,
        CancellationToken cancellationToken = default);

    Task<CompanyResponseDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CompanyResponseDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<CompanyResponseDto?> UpdateAsync(
        Guid id,
        UpdateCompanyDto dto,
        CancellationToken cancellationToken = default);

    Task<bool> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
