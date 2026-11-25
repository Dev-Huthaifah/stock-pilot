using Microsoft.EntityFrameworkCore;
using StockPilot.Api.Data;
using StockPilot.Api.Data.Seeding;
using StockPilot.Api.Enums;
using StockPilot.Api.Models;
using StockPilot.Api.Security;

namespace StockPilot.Api.Tests.Data.Seeding;

public class CompanyRolePermissionSeederTests
{
    [Fact]
    public async Task SeedCompanyAsync_SeedsExpectedPermissionsForAllRoles()
    {
        await using var db = CreateDbContext();

        var company = await CreateCompanyAsync(db);

        await PermissionSeeder.SeedAsync(db);

        await CompanyRolePermissionSeeder.SeedCompanyAsync(
            db,
            company.Id);

        var moderatorPermissions =
            await GetPermissionCodesAsync(
                db,
                company.Id,
                CompanyRole.CompanyModerator);

        var warehouseModeratorPermissions =
            await GetPermissionCodesAsync(
                db,
                company.Id,
                CompanyRole.WarehouseModerator);

        var employeePermissions =
            await GetPermissionCodesAsync(
                db,
                company.Id,
                CompanyRole.WarehouseEmployee);

        Assert.Equal(18, moderatorPermissions.Count);
        Assert.Equal(
            PermissionCatalog.All.Select(x => x.Code).OrderBy(x => x),
            moderatorPermissions.OrderBy(x => x));

        Assert.Equal(8, warehouseModeratorPermissions.Count);

        Assert.Contains(
            "stock.transfer.view",
            warehouseModeratorPermissions);

        Assert.Contains(
            "stock.transfer.create",
            warehouseModeratorPermissions);

        Assert.DoesNotContain(
            "stock.transfer.post",
            warehouseModeratorPermissions);

        Assert.DoesNotContain(
            "stock.transfer.cancel",
            warehouseModeratorPermissions);

        Assert.Equal(5, employeePermissions.Count);

        Assert.DoesNotContain(
            "stock.transfer.view",
            employeePermissions);

        Assert.DoesNotContain(
            "stock.adjust",
            employeePermissions);
    }

    [Fact]
    public async Task SeedCompanyAsync_IsIdempotent()
    {
        await using var db = CreateDbContext();

        var company = await CreateCompanyAsync(db);

        await PermissionSeeder.SeedAsync(db);

        await CompanyRolePermissionSeeder.SeedCompanyAsync(
            db,
            company.Id);

        var firstCount = await db.CompanyRolePermissions.CountAsync();

        await CompanyRolePermissionSeeder.SeedCompanyAsync(
            db,
            company.Id);

        var secondCount = await db.CompanyRolePermissions.CountAsync();

        Assert.Equal(31, firstCount);
        Assert.Equal(firstCount, secondCount);
    }

    [Fact]
    public async Task SeedAllAsync_SeedsPermissionsForExistingCompanies()
    {
        await using var db = CreateDbContext();

        await PermissionSeeder.SeedAsync(db);

        db.Companies.AddRange(
            new Company
            {
                Id = Guid.NewGuid(),
                Name = "Company One",
                IsActive = true
            },
            new Company
            {
                Id = Guid.NewGuid(),
                Name = "Company Two",
                IsActive = true
            });

        await db.SaveChangesAsync();

        await CompanyRolePermissionSeeder.SeedAllAsync(db);

        var companyCount = await db.Companies.CountAsync();
        var grantCount = await db.CompanyRolePermissions.CountAsync();

        Assert.Equal(2, companyCount);
        Assert.Equal(62, grantCount);
    }

    [Fact]
    public async Task SeedCompanyAsync_MissingPermissionCatalog_ThrowsException()
    {
        await using var db = CreateDbContext();

        var company = await CreateCompanyAsync(db);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => CompanyRolePermissionSeeder.SeedCompanyAsync(
                db,
                company.Id));

        Assert.Contains("Missing permissions", exception.Message);
    }

    private static StockPilotDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<StockPilotDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        return new StockPilotDbContext(options);
    }

    private static async Task<Company> CreateCompanyAsync(
        StockPilotDbContext db)
    {
        var company = new Company
        {
            Id = Guid.NewGuid(),
            Name = "Test Company",
            IsActive = true
        };

        db.Companies.Add(company);

        await db.SaveChangesAsync();

        return company;
    }

    private static async Task<List<string>> GetPermissionCodesAsync(
        StockPilotDbContext db,
        Guid companyId,
        CompanyRole role)
    {
        return await db.CompanyRolePermissions
            .Where(x =>
                x.CompanyId == companyId &&
                x.Role == role)
            .Join(
                db.Permissions,
                grant => grant.PermissionId,
                permission => permission.Id,
                (grant, permission) => permission.Code)
            .ToListAsync();
    }
}
