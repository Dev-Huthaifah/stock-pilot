
using StockPilot.Api.DTOs.Auth;

namespace StockPilot.Api.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default);

    Task<LoginResponseDto?> ChangeTemporaryPasswordAsync(
        Guid userId,
        ChangeTemporaryPasswordRequestDto request,
        CancellationToken cancellationToken = default);
}
