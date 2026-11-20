using Microsoft.EntityFrameworkCore;
using StockPilot.Api.Security;

namespace StockPilot.Api.Data.Seeding;

public static class PermissionSeeder
{
    public static async Task SeedAsync(
        StockPilotDbContext db,
        CancellationToken cancellationToken = default)
    {
        var existingCodes = await db.Permissions
            .Select(x => x.Code)
            .ToListAsync(cancellationToken);

        var existingSet = existingCodes.ToHashSet(
            StringComparer.OrdinalIgnoreCase);

        var missingPermissions = PermissionCatalog.All
            .Where(permission => !existingSet.Contains(permission.Code))
            .Select(permission => new Models.Permission
            {
                Code = permission.Code,
                Name = permission.Name,
                Category = permission.Category,
                Description = permission.Description,
                IsActive = true
            })
            .ToList();

        if (missingPermissions.Count == 0)
            return;

        db.Permissions.AddRange(missingPermissions);

        await db.SaveChangesAsync(cancellationToken);
    }
}