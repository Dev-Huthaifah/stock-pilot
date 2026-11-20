using StockPilot.Api.Enums;

namespace StockPilot.Api.Models;

public class CompanyMembership
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public Guid CompanyId { get; set; }

    public Guid? WarehouseId { get; set; }

    public CompanyRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;

    public Company Company { get; set; } = null!;

    public Warehouse? Warehouse { get; set; }
    public ICollection<CompanyUserPermissionOverride> PermissionOverrides { get; set; }
    = new List<CompanyUserPermissionOverride>();
}