using System;

namespace StockPilot.Api.Models;

public class CompanyUserPermissionOverride
{
    public Guid CompanyMembershipId { get; set; }

    public Guid PermissionId { get; set; }

    public bool IsAllowed { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public CompanyMembership CompanyMembership { get; set; } = null!;

    public Permission Permission { get; set; } = null!;
}
