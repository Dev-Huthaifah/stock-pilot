using StockPilot.Api.Data.Seeding;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StockPilot.Api.Data;
using StockPilot.Api.DTOs.Platform;
using StockPilot.Api.Enums;
using StockPilot.Api.Models;
using StockPilot.Api.Services.Interfaces;

namespace StockPilot.Api.Services.Implementations;

public sealed class PlatformCompanyService : IPlatformCompanyService
{
    private readonly StockPilotDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public PlatformCompanyService(
        StockPilotDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<CreateCompanyResponseDto> CreateCompanyAsync(
        CreateCompanyRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var adminEmail = request.AdminEmail.Trim();

        if (await _userManager.FindByEmailAsync(adminEmail) is not null)
        {
            throw new InvalidOperationException(
                "An account with this email already exists.");
        }

        var temporaryPassword = GenerateTemporaryPassword();

        await using var transaction =
            await _db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var company = new Company
            {
                Name = request.CompanyName.Trim(),
                Email = request.CompanyEmail?.Trim(),
                Phone = request.CompanyPhone?.Trim(),
                IsActive = true
            };

            _db.Companies.Add(company);

            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = request.AdminFullName.Trim(),
                IsActive = true,
                MustChangePassword = true
            };

            var createUserResult = await _userManager.CreateAsync(
                admin,
                temporaryPassword);

            if (!createUserResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(
                        "; ",
                        createUserResult.Errors.Select(x => x.Description)));
            }

            var membership = new CompanyMembership
            {
                UserId = admin.Id,
                CompanyId = company.Id,
                WarehouseId = null,
                Role = CompanyRole.CompanyModerator,
                IsActive = true
            };

            _db.CompanyMemberships.Add(membership);

            await _db.SaveChangesAsync(cancellationToken);

            await CompanyRolePermissionSeeder.SeedCompanyAsync(
                _db,
                company.Id,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return new CreateCompanyResponseDto
            {
                CompanyId = company.Id,
                CompanyName = company.Name,
                AdminUserId = admin.Id,
                AdminEmail = admin.Email!,
                TemporaryPassword = temporaryPassword
            };
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static string GenerateTemporaryPassword()
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string symbols = "!@#$%&*+-_?";

        var all = upper + lower + digits + symbols;

        var chars = new char[24];

        chars[0] = upper[RandomNumberGenerator.GetInt32(upper.Length)];
        chars[1] = lower[RandomNumberGenerator.GetInt32(lower.Length)];
        chars[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
        chars[3] = symbols[RandomNumberGenerator.GetInt32(symbols.Length)];

        for (var i = 4; i < chars.Length; i++)
        {
            chars[i] = all[RandomNumberGenerator.GetInt32(all.Length)];
        }

        for (var i = chars.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }

    public async Task<CreateCompanyResponseDto> CreateCompanyAdminAsync(
        Guid companyId,
        CreateCompanyAdminRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var adminEmail = request.AdminEmail.Trim();

        var company = await _db.Companies
            .SingleOrDefaultAsync(
                x => x.Id == companyId,
                cancellationToken);

        if (company is null)
        {
            throw new KeyNotFoundException("Company not found.");
        }

        if (!company.IsActive)
        {
            throw new InvalidOperationException("Company is inactive.");
        }

        if (await _userManager.FindByEmailAsync(adminEmail) is not null)
        {
            throw new InvalidOperationException(
                "An account with this email already exists.");
        }

        var existingAdmin = await _db.CompanyMemberships
            .AnyAsync(
                x => x.CompanyId == companyId &&
                    x.Role == CompanyRole.CompanyModerator &&
                    x.IsActive,
                cancellationToken);

        if (existingAdmin)
        {
            throw new InvalidOperationException(
                "This company already has an active Company Moderator.");
        }

        var temporaryPassword = GenerateTemporaryPassword();

        await using var transaction =
            await _db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = request.AdminFullName.Trim(),
                IsActive = true,
                MustChangePassword = true
            };

            var createUserResult = await _userManager.CreateAsync(
                admin,
                temporaryPassword);

            if (!createUserResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(
                        "; ",
                        createUserResult.Errors.Select(x => x.Description)));
            }

            var membership = new CompanyMembership
            {
                UserId = admin.Id,
                CompanyId = company.Id,
                WarehouseId = null,
                Role = CompanyRole.CompanyModerator,
                IsActive = true
            };

            _db.CompanyMemberships.Add(membership);

            await _db.SaveChangesAsync(cancellationToken);

            await CompanyRolePermissionSeeder.SeedCompanyAsync(
                _db,
                company.Id,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return new CreateCompanyResponseDto
            {
                CompanyId = company.Id,
                CompanyName = company.Name,
                AdminUserId = admin.Id,
                AdminEmail = admin.Email!,
                TemporaryPassword = temporaryPassword
            };
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}