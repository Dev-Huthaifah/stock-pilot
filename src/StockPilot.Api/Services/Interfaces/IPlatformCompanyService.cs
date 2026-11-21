using StockPilot.Api.DTOs.Platform;

namespace StockPilot.Api.Services.Interfaces;

public interface IPlatformCompanyService
{
    Task<CreateCompanyResponseDto> CreateCompanyAsync(
        CreateCompanyRequestDto request,
        CancellationToken cancellationToken = default);
    Task<CreateCompanyResponseDto> CreateCompanyAdminAsync(
    Guid companyId,
    CreateCompanyAdminRequestDto request,
    CancellationToken cancellationToken = default);
}