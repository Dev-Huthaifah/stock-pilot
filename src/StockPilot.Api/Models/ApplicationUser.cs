using Microsoft.AspNetCore.Identity;

namespace StockPilot.Api.Models;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CompanyMembership> Memberships { get; set; }
    = new List<CompanyMembership>();
}