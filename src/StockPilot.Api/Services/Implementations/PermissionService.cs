using StockPilot.Api.Enums;
using Microsoft.EntityFrameworkCore;
using StockPilot.Api.Data;
using StockPilot.Api.Services.Interfaces;

namespace StockPilot.Api.Services.Implementations;

public sealed class PermissionService : IPermissionService
{
    private readonly StockPilotDbContext _db;

    public PermissionService(StockPilotDbContext db)
    {
        _db = db;
    }

    public async Task<bool> HasPermissionAsync(
        Guid userId,
        Guid companyId,
        string permissionCode,
        Guid? warehouseId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(permissionCode))
            return false;

        var membership = await GetActiveMembershipAsync(
            userId, companyId, cancellationToken);

        if (membership is null)
            return false;

        if (membership.WarehouseId.HasValue)
        {
            var assignedWarehouseIsActive = await _db.Warehouses
                .AsNoTracking()
                .AnyAsync(w =>
                    w.Id == membership.WarehouseId.Value &&
                    w.CompanyId == companyId &&
                    w.IsActive,
                    cancellationToken);

            if (!assignedWarehouseIsActive)
                return false;

            if (!warehouseId.HasValue ||
                membership.WarehouseId.Value != warehouseId.Value)
            {
                return false;
            }
        }
        else if (warehouseId.HasValue)
        {
            var warehouseExists = await _db.Warehouses
                .AsNoTracking()
                .AnyAsync(w =>
                    w.Id == warehouseId.Value &&
                    w.CompanyId == companyId &&
                    w.IsActive,
                    cancellationToken);

            if (!warehouseExists)
                return false;
        }

        return await ResolvePermissionAsync(
            membership.Id,
            membership.Role,
            companyId,
            permissionCode,
            cancellationToken);
    }

    public async Task<bool> HasCompanyPermissionAsync(
        Guid userId,
        Guid companyId,
        string permissionCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(permissionCode))
            return false;

        var membership = await GetActiveMembershipAsync(
            userId, companyId, cancellationToken);

        if (membership is null)
            return false;

        if (membership.WarehouseId.HasValue)
        {
            var assignedWarehouseIsActive = await _db.Warehouses
                .AsNoTracking()
                .AnyAsync(w =>
                    w.Id == membership.WarehouseId.Value &&
                    w.CompanyId == companyId &&
                    w.IsActive,
                    cancellationToken);

            if (!assignedWarehouseIsActive)
                return false;
        }

        return await ResolvePermissionAsync(
            membership.Id,
            membership.Role,
            companyId,
            permissionCode,
            cancellationToken);
    }

    private async Task<MembershipInfo?> GetActiveMembershipAsync(
        Guid userId,
        Guid companyId,
        CancellationToken cancellationToken)
    {
        return await _db.CompanyMemberships
            .AsNoTracking()
            .Where(m =>
                m.UserId == userId &&
                m.CompanyId == companyId &&
                m.IsActive &&
                m.User.IsActive &&
                m.Company.IsActive)
            .Select(m => new MembershipInfo(
                m.Id,
                m.Role,
                m.WarehouseId))
            .SingleOrDefaultAsync(cancellationToken);
    }

    private async Task<bool> ResolvePermissionAsync(
        Guid membershipId,
        CompanyRole role,
        Guid companyId,
        string permissionCode,
        CancellationToken cancellationToken)
    {
        var permissionId = await _db.Permissions
            .AsNoTracking()
            .Where(p =>
                p.Code == permissionCode &&
                p.IsActive)
            .Select(p => (Guid?)p.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (!permissionId.HasValue)
            return false;

        var userOverride = await _db.CompanyUserPermissionOverrides
            .AsNoTracking()
            .Where(x =>
                x.CompanyMembershipId == membershipId &&
                x.PermissionId == permissionId.Value)
            .Select(x => (bool?)x.IsAllowed)
            .SingleOrDefaultAsync(cancellationToken);

        if (userOverride.HasValue)
            return userOverride.Value;

        return await _db.CompanyRolePermissions
            .AsNoTracking()
            .AnyAsync(x =>
                x.CompanyId == companyId &&
                x.Role == role &&
                x.PermissionId == permissionId.Value,
                cancellationToken);
    }

    private sealed record MembershipInfo(
        Guid Id,
        CompanyRole Role,
        Guid? WarehouseId);
}