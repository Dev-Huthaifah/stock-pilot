using Microsoft.EntityFrameworkCore;
using StockPilot.Api.Enums;
using StockPilot.Api.Models;
using StockPilot.Api.Security;

namespace StockPilot.Api.Data.Seeding;

public static class CompanyRolePermissionSeeder
{
    private static readonly IReadOnlyDictionary<CompanyRole, string[]>
        RolePermissions = new Dictionary<CompanyRole, string[]>
        {
            [CompanyRole.CompanyModerator] =
                PermissionCatalog.All
                    .Select(x => x.Code)
                    .ToArray(),

            [CompanyRole.WarehouseModerator] =
            [
                "products.view",
                "warehouses.view",
                "stock.view",
                "stock.in",
                "stock.out",
                "stock.adjust",
                "stock.transfer.view",
                "stock.transfer.create"
            ],

            [CompanyRole.WarehouseEmployee] =
            [
                "products.view",
                "warehouses.view",
                "stock.view",
                "stock.in",
                "stock.out"
            ]
        };

    public static async Task SeedAllAsync(
        StockPilotDbContext db,
        CancellationToken cancellationToken = default)
    {
        var companyIds = await db.Companies
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        foreach (var companyId in companyIds)
        {
            await SeedCompanyAsync(
                db,
                companyId,
                cancellationToken);
        }
    }

    public static async Task SeedCompanyAsync(
        StockPilotDbContext db,
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        var companyExists = await db.Companies
            .AnyAsync(x => x.Id == companyId, cancellationToken);

        if (!companyExists)
        {
            throw new InvalidOperationException(
                $"Company {companyId} does not exist.");
        }

        var requiredCodes = RolePermissions.Values
            .SelectMany(x => x)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var permissions = await db.Permissions
            .Where(x => requiredCodes.Contains(x.Code))
            .ToDictionaryAsync(
                x => x.Code,
                StringComparer.OrdinalIgnoreCase,
                cancellationToken);

        var missingCodes = requiredCodes
            .Where(code => !permissions.ContainsKey(code))
            .ToArray();

        if (missingCodes.Length > 0)
        {
            throw new InvalidOperationException(
                $"Missing permissions: {string.Join(", ", missingCodes)}");
        }

        var existingGrants = await db.CompanyRolePermissions
            .Where(x => x.CompanyId == companyId)
            .Select(x => new
            {
                x.Role,
                x.PermissionId
            })
            .ToListAsync(cancellationToken);

        var existingSet = existingGrants
            .Select(x => (x.Role, x.PermissionId))
            .ToHashSet();

        var newGrants = new List<CompanyRolePermission>();

        foreach (var roleDefinition in RolePermissions)
        {
            foreach (var permissionCode in roleDefinition.Value)
            {
                var permissionId =
                    permissions[permissionCode].Id;

                if (existingSet.Contains(
                    (roleDefinition.Key, permissionId)))
                {
                    continue;
                }

                newGrants.Add(new CompanyRolePermission
                {
                    CompanyId = companyId,
                    Role = roleDefinition.Key,
                    PermissionId = permissionId
                });
            }
        }

        if (newGrants.Count == 0)
            return;

        db.CompanyRolePermissions.AddRange(newGrants);

        await db.SaveChangesAsync(cancellationToken);
    }
}
