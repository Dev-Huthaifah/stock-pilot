using StockPilot.Api.Enums;

namespace StockPilot.Api.Models;

public class CompanyRolePermission
{
    public Guid CompanyId { get; set; }

    public CompanyRole Role { get; set; }

    public Guid PermissionId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Company Company { get; set; } = null!;

    public Permission Permission { get; set; } = null!;
}