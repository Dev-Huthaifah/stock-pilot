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

        var membership = await _db.CompanyMemberships
            .AsNoTracking()
            .Where(m =>
                m.UserId == userId &&
                m.CompanyId == companyId &&
                m.IsActive &&
                m.User.IsActive &&
                m.Company.IsActive)
            .Select(m => new
            {
                m.Id,
                m.Role,
                m.WarehouseId
            })
            .SingleOrDefaultAsync(cancellationToken);

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

        if (membership.WarehouseId.HasValue)
        {
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
                x.CompanyMembershipId == membership.Id &&
                x.PermissionId == permissionId.Value)
            .Select(x => (bool?)x.IsAllowed)
            .SingleOrDefaultAsync(cancellationToken);

        if (userOverride.HasValue)
            return userOverride.Value;

        return await _db.CompanyRolePermissions
            .AsNoTracking()
            .AnyAsync(x =>
                x.CompanyId == companyId &&
                x.Role == membership.Role &&
                x.PermissionId == permissionId.Value,
                cancellationToken);
    }
}