
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StockPilot.Api.Data;
using StockPilot.Api.Data.Seeding;
using StockPilot.Api.DTOs.Auth;
using StockPilot.Api.Models;
using StockPilot.Api.Services.Interfaces;

namespace StockPilot.Api.Services.Implementations;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly StockPilotDbContext _db;
    private readonly ITokenService _tokenService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        StockPilotDbContext db,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<LoginResponseDto?> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();

        var user = await _userManager.FindByEmailAsync(email);

        if (user is null || !user.IsActive)
            return null;

        var passwordResult =
            await _signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: true);

        if (!passwordResult.Succeeded)
            return null;

        if (await _userManager.IsInRoleAsync(
            user,
            PlatformAdminBootstrapper.RoleName))
        {
            var platformToken =
                _tokenService.GeneratePlatformAdminToken(user);

            return new LoginResponseDto
            {
                AccessToken = platformToken.AccessToken,
                TokenType = platformToken.TokenType,
                ExpiresAt = platformToken.ExpiresAt,
                RequiresPasswordChange = false
            };
        }

        var membership = await _db.CompanyMemberships
            .Include(x => x.User)
            .Include(x => x.Company)
            .Include(x => x.Warehouse)
            .SingleOrDefaultAsync(
                x => x.UserId == user.Id,
                cancellationToken);

        if (membership is null ||
            !membership.IsActive ||
            !membership.Company.IsActive)
        {
            return null;
        }

        if (membership.WarehouseId.HasValue &&
            (membership.Warehouse is null ||
            !membership.Warehouse.IsActive))
        {
            return null;
        }

        if (user.MustChangePassword)
        {
            var challenge =
                _tokenService.GeneratePasswordChangeToken(user);

            return new LoginResponseDto
            {
                AccessToken = challenge.AccessToken,
                TokenType = challenge.TokenType,
                ExpiresAt = challenge.ExpiresAt,
                RequiresPasswordChange = true
            };
        }

        var token = _tokenService.GenerateToken(membership);

        return new LoginResponseDto
        {
            AccessToken = token.AccessToken,
            TokenType = token.TokenType,
            ExpiresAt = token.ExpiresAt,
            RequiresPasswordChange = false
        };
    }

    public async Task<LoginResponseDto?> ChangeTemporaryPasswordAsync(
        Guid userId,
        ChangeTemporaryPasswordRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null || !user.IsActive || !user.MustChangePassword)
            return null;

        var membership = await _db.CompanyMemberships
            .Include(x => x.User)
            .Include(x => x.Company)
            .Include(x => x.Warehouse)
            .SingleOrDefaultAsync(
                x => x.UserId == user.Id,
                cancellationToken);

        if (membership is null ||
            !membership.IsActive ||
            !membership.Company.IsActive)
        {
            return null;
        }

        if (membership.WarehouseId.HasValue &&
            (membership.Warehouse is null ||
            !membership.Warehouse.IsActive))
        {
            return null;
        }

        await using var transaction =
            await _db.Database.BeginTransactionAsync(cancellationToken);

        var passwordResult = await _userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword);

        if (!passwordResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);
            return null;
        }

        user.MustChangePassword = false;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);
            return null;
        }

        await transaction.CommitAsync(cancellationToken);

        var token = _tokenService.GenerateToken(membership);

        return new LoginResponseDto
        {
            AccessToken = token.AccessToken,
            TokenType = token.TokenType,
            ExpiresAt = token.ExpiresAt,
            RequiresPasswordChange = false
        };
    }
}
