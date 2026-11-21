using System.ComponentModel.DataAnnotations;

namespace StockPilot.Api.DTOs.Platform;

public sealed class CreateCompanyAdminRequestDto
{
    [Required]
    [MaxLength(200)]
    public string AdminFullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string AdminEmail { get; set; } = string.Empty;
}