namespace StockPilot.Api.DTOs.Platform;

public sealed class CreateCompanyResponseDto
{
    public Guid CompanyId { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public Guid AdminUserId { get; set; }

    public string AdminEmail { get; set; } = string.Empty;

    public string TemporaryPassword { get; set; } = string.Empty;
}