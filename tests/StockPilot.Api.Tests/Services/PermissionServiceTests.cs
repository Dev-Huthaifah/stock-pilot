
using Microsoft.EntityFrameworkCore;
using StockPilot.Api.Data;
using StockPilot.Api.Enums;
using StockPilot.Api.Models;
using StockPilot.Api.Services.Implementations;

namespace StockPilot.Api.Tests.Services;

public class PermissionServiceTests
{
    private const string PermissionCode = "products.view";

    [Fact]
    public async Task HasPermissionAsync_RoleHasPermission_ReturnsTrue()
    {
        var scenario = await CreateScenarioAsync(
            grantRolePermission: true);

        using var db = scenario.Db;

        var service = new PermissionService(db);

        var result = await service.HasPermissionAsync(
            scenario.Ids.UserId,
            scenario.Ids.CompanyId,
            PermissionCode);

        Assert.True(result);
    }

    [Fact]
    public async Task HasPermissionAsync_RoleDoesNotHavePermission_ReturnsFalse()
    {
        var scenario = await CreateScenarioAsync();

        using var db = scenario.Db;

        var service = new PermissionService(db);

        var result = await service.HasPermissionAsync(
            scenario.Ids.UserId,
            scenario.Ids.CompanyId,
            PermissionCode);

        Assert.False(result);
    }

    [Fact]
    public async Task HasPermissionAsync_OverrideAllow_ReturnsTrue()
    {
        var scenario = await CreateScenarioAsync();

        using var db = scenario.Db;

        db.CompanyUserPermissionOverrides.Add(
            new CompanyUserPermissionOverride
            {
                CompanyMembershipId = scenario.Ids.MembershipId,
                PermissionId = scenario.Ids.PermissionId,
                IsAllowed = true
            });

        await db.SaveChangesAsync();

        var service = new PermissionService(db);

        var result = await service.HasPermissionAsync(
            scenario.Ids.UserId,
            scenario.Ids.CompanyId,
            PermissionCode);

        Assert.True(result);
    }

    [Fact]
    public async Task HasPermissionAsync_OverrideDeny_TakesPrecedenceOverRole()
    {
        var scenario = await CreateScenarioAsync(
            grantRolePermission: true);

        using var db = scenario.Db;

        db.CompanyUserPermissionOverrides.Add(
            new CompanyUserPermissionOverride
            {
                CompanyMembershipId = scenario.Ids.MembershipId,
                PermissionId = scenario.Ids.PermissionId,
                IsAllowed = false
            });

        await db.SaveChangesAsync();

        var service = new PermissionService(db);

        var result = await service.HasPermissionAsync(
            scenario.Ids.UserId,
            scenario.Ids.CompanyId,
            PermissionCode);

        Assert.False(result);
    }

    [Fact]
    public async Task HasPermissionAsync_UnknownPermission_ReturnsFalse()
    {
        var scenario = await CreateScenarioAsync(
            grantRolePermission: true);

        using var db = scenario.Db;

        var service = new PermissionService(db);

        var result = await service.HasPermissionAsync(
            scenario.Ids.UserId,
            scenario.Ids.CompanyId,
            "unknown.permission");

        Assert.False(result);
    }

    [Fact]
    public async Task HasPermissionAsync_InactiveUser_ReturnsFalse()
    {
        var scenario = await CreateScenarioAsync(
            userActive: false,
            grantRolePermission: true);

        using var db = scenario.Db;

        var service = new PermissionService(db);

        var result = await service.HasPermissionAsync(
            scenario.Ids.UserId,
            scenario.Ids.CompanyId,
            PermissionCode);

        Assert.False(result);
    }

    [Fact]
    public async Task HasPermissionAsync_InactiveMembership_ReturnsFalse()
    {
        var scenario = await CreateScenarioAsync(
            membershipActive: false,
            grantRolePermission: true);

        using var db = scenario.Db;

        var service = new PermissionService(db);

        var result = await service.HasPermissionAsync(
            scenario.Ids.UserId,
            scenario.Ids.CompanyId,
            PermissionCode);

        Assert.False(result);
    }

    [Fact]
    public async Task HasPermissionAsync_DifferentCompany_ReturnsFalse()
    {
        var scenario = await CreateScenarioAsync(
            grantRolePermission: true);

        using var db = scenario.Db;

        var service = new PermissionService(db);

        var differentCompanyId = Guid.NewGuid();

        var result = await service.HasPermissionAsync(
            scenario.Ids.UserId,
            differentCompanyId,
            PermissionCode);

        Assert.False(result);
    }

    [Fact]
    public async Task HasPermissionAsync_WarehouseEmployeeAccessesAssignedWarehouse()
    {
        var scenario = await CreateScenarioAsync(
            role: CompanyRole.WarehouseEmployee,
            assignedWarehouse: true,
            grantRolePermission: true);

        using var db = scenario.Db;

        var service = new PermissionService(db);

        var result = await service.HasPermissionAsync(
            scenario.Ids.UserId,
            scenario.Ids.CompanyId,
            PermissionCode,
            scenario.Ids.WarehouseId);

        Assert.True(result);
    }

    [Fact]
    public async Task HasPermissionAsync_WarehouseEmployeeCannotAccessAnotherWarehouse()
    {
        var scenario = await CreateScenarioAsync(
            role: CompanyRole.WarehouseEmployee,
            assignedWarehouse: true,
            grantRolePermission: true);

        using var db = scenario.Db;

        var service = new PermissionService(db);

        var result = await service.HasPermissionAsync(
            scenario.Ids.UserId,
            scenario.Ids.CompanyId,
            PermissionCode,
            scenario.Ids.OtherWarehouseId);

        Assert.False(result);
    }

    [Fact]
    public async Task HasPermissionAsync_CompanyModeratorCanAccessCompanyWarehouse()
    {
        var scenario = await CreateScenarioAsync(
            role: CompanyRole.CompanyModerator,
            grantRolePermission: true);

        using var db = scenario.Db;

        var service = new PermissionService(db);

        var result = await service.HasPermissionAsync(
            scenario.Ids.UserId,
            scenario.Ids.CompanyId,
            PermissionCode,
            scenario.Ids.WarehouseId);

        Assert.True(result);
    }

    [Fact]
    public async Task HasCompanyPermissionAsync_WarehouseEmployeeCanViewProducts_ReturnsTrue()
    {
        var scenario = await CreateScenarioAsync(
            role: CompanyRole.WarehouseEmployee,
            assignedWarehouse: true,
            grantRolePermission: true);

        using var db = scenario.Db;
        var service = new PermissionService(db);

        var result = await service.HasCompanyPermissionAsync(
            scenario.Ids.UserId,
            scenario.Ids.CompanyId,
            PermissionCode);

        Assert.True(result);
    }

    [Fact]
    public async Task HasCompanyPermissionAsync_WarehouseModeratorCanViewProducts_ReturnsTrue()
    {
        var scenario = await CreateScenarioAsync(
            role: CompanyRole.WarehouseModerator,
            assignedWarehouse: true,
            grantRolePermission: true);

        using var db = scenario.Db;
        var service = new PermissionService(db);

        var result = await service.HasCompanyPermissionAsync(
            scenario.Ids.UserId,
            scenario.Ids.CompanyId,
            PermissionCode);

        Assert.True(result);
    }

    [Fact]
    public async Task HasCompanyPermissionAsync_RoleDoesNotHavePermission_ReturnsFalse()
    {
        var scenario = await CreateScenarioAsync(
            role: CompanyRole.WarehouseEmployee,
            assignedWarehouse: true);

        using var db = scenario.Db;
        var service = new PermissionService(db);

        var result = await service.HasCompanyPermissionAsync(
            scenario.Ids.UserId,
            scenario.Ids.CompanyId,
            PermissionCode);

        Assert.False(result);
    }

    [Fact]
    public async Task HasCompanyPermissionAsync_OverrideDeny_TakesPrecedenceOverRole()
    {
        var scenario = await CreateScenarioAsync(
            role: CompanyRole.WarehouseEmployee,
            assignedWarehouse: true,
            grantRolePermission: true);

        using var db = scenario.Db;

        db.CompanyUserPermissionOverrides.Add(
            new CompanyUserPermissionOverride
            {
                CompanyMembershipId = scenario.Ids.MembershipId,
                PermissionId = scenario.Ids.PermissionId,
                IsAllowed = false
            });

        await db.SaveChangesAsync();

        var service = new PermissionService(db);

        var result = await service.HasCompanyPermissionAsync(
            scenario.Ids.UserId,
            scenario.Ids.CompanyId,
            PermissionCode);

        Assert.False(result);
    }

    [Fact]
    public async Task HasCompanyPermissionAsync_InactiveMembership_ReturnsFalse()
    {
        var scenario = await CreateScenarioAsync(
            role: CompanyRole.WarehouseEmployee,
            assignedWarehouse: true,
            membershipActive: false,
            grantRolePermission: true);

        using var db = scenario.Db;
        var service = new PermissionService(db);

        var result = await service.HasCompanyPermissionAsync(
            scenario.Ids.UserId,
            scenario.Ids.CompanyId,
            PermissionCode);

        Assert.False(result);
    }

    [Fact]
    public async Task HasCompanyPermissionAsync_InactiveAssignedWarehouse_ReturnsFalse()
    {
        var scenario = await CreateScenarioAsync(
            role: CompanyRole.WarehouseEmployee,
            assignedWarehouse: true,
            assignedWarehouseActive: false,
            grantRolePermission: true);

        using var db = scenario.Db;
        var service = new PermissionService(db);

        var result = await service.HasCompanyPermissionAsync(
            scenario.Ids.UserId,
            scenario.Ids.CompanyId,
            PermissionCode);

        Assert.False(result);
    }

    private static async Task<(StockPilotDbContext Db, TestIds Ids)>
        CreateScenarioAsync(
            bool grantRolePermission = false,
            CompanyRole role = CompanyRole.CompanyModerator,
            bool companyActive = true,
            bool assignedWarehouseActive = true,
            bool permissionActive = true,
            bool assignedWarehouse = false,
            bool userActive = true,
            bool membershipActive = true)
    {
        var options = new DbContextOptionsBuilder<StockPilotDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new StockPilotDbContext(options);

        var companyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var membershipId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var otherWarehouseId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        var company = new Company
        {
            Id = companyId,
            Name = "Test Company",
            IsActive = companyActive
        };

        var warehouse = new Warehouse
        {
            Id = warehouseId,
            CompanyId = companyId,
            Name = "Main Warehouse",
            Code = "WH-001",
            IsActive = assignedWarehouseActive
        };

        var otherWarehouse = new Warehouse
        {
            Id = otherWarehouseId,
            CompanyId = companyId,
            Name = "Second Warehouse",
            Code = "WH-002",
            IsActive = true
        };

        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "test@example.com",
            Email = "test@example.com",
            FullName = "Test User",
            IsActive = userActive
        };

        var permission = new Permission
        {
            Id = permissionId,
            Code = PermissionCode,
            Name = "View Products",
            Category = "Products",
            IsActive = permissionActive
        };

        var membership = new CompanyMembership
        {
            Id = membershipId,
            UserId = userId,
            CompanyId = companyId,
            WarehouseId = assignedWarehouse ? warehouseId : null,
            Role = role,
            IsActive = membershipActive
        };

        db.Companies.Add(company);
        db.Warehouses.AddRange(warehouse, otherWarehouse);
        db.Users.Add(user);
        db.Permissions.Add(permission);
        db.CompanyMemberships.Add(membership);

        if (grantRolePermission)
        {
            db.CompanyRolePermissions.Add(
                new CompanyRolePermission
                {
                    CompanyId = companyId,
                    Role = role,
                    PermissionId = permissionId
                });
        }

        await db.SaveChangesAsync();

        return (
            db,
            new TestIds(
                userId,
                companyId,
                membershipId,
                warehouseId,
                otherWarehouseId,
                permissionId)
        );
    }

    private sealed record TestIds(
        Guid UserId,
        Guid CompanyId,
        Guid MembershipId,
        Guid WarehouseId,
        Guid OtherWarehouseId,
        Guid PermissionId);
    
    [Fact]
public async Task HasPermissionAsync_InactiveCompany_ReturnsFalse()
{
    var scenario = await CreateScenarioAsync(
        companyActive: false,
        grantRolePermission: true);

    using var db = scenario.Db;
    var service = new PermissionService(db);

    var result = await service.HasPermissionAsync(
        scenario.Ids.UserId,
        scenario.Ids.CompanyId,
        PermissionCode);

    Assert.False(result);
}

[Fact]
public async Task HasPermissionAsync_InactiveWarehouse_ReturnsFalse()
{
    var scenario = await CreateScenarioAsync(
        role: CompanyRole.WarehouseEmployee,
        assignedWarehouse: true,
        assignedWarehouseActive: false,
        grantRolePermission: true);

    using var db = scenario.Db;
    var service = new PermissionService(db);

    var result = await service.HasPermissionAsync(
        scenario.Ids.UserId,
        scenario.Ids.CompanyId,
        PermissionCode,
        scenario.Ids.WarehouseId);

    Assert.False(result);
}

[Fact]
public async Task HasPermissionAsync_InactivePermission_ReturnsFalse()
{
    var scenario = await CreateScenarioAsync(
        permissionActive: false,
        grantRolePermission: true);

    using var db = scenario.Db;
    var service = new PermissionService(db);

    var result = await service.HasPermissionAsync(
        scenario.Ids.UserId,
        scenario.Ids.CompanyId,
        PermissionCode);

    Assert.False(result);
}

[Fact]
public async Task HasPermissionAsync_WarehouseEmployeeWithoutWarehouseContext_ReturnsFalse()
{
    var scenario = await CreateScenarioAsync(
        role: CompanyRole.WarehouseEmployee,
        assignedWarehouse: true,
        grantRolePermission: true);

    using var db = scenario.Db;
    var service = new PermissionService(db);

    var result = await service.HasPermissionAsync(
        scenario.Ids.UserId,
        scenario.Ids.CompanyId,
        PermissionCode);

    Assert.False(result);
}

[Fact]
public async Task HasPermissionAsync_CompanyModeratorCannotAccessAnotherCompanyWarehouse()
{
    var scenario = await CreateScenarioAsync(
        grantRolePermission: true);

    using var db = scenario.Db;
    var service = new PermissionService(db);

    var result = await service.HasPermissionAsync(
        scenario.Ids.UserId,
        scenario.Ids.CompanyId,
        PermissionCode,
        Guid.NewGuid());

    Assert.False(result);
}
}
