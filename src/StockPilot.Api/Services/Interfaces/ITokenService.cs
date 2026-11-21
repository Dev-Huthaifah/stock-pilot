using StockPilot.Api.DTOs.Auth;
using StockPilot.Api.Models;

namespace StockPilot.Api.Services.Interfaces;

public interface ITokenService
{
    TokenResponseDto GenerateToken(CompanyMembership membership);

    TokenResponseDto GeneratePlatformAdminToken(ApplicationUser user);

    TokenResponseDto GeneratePasswordChangeToken(ApplicationUser user);
}