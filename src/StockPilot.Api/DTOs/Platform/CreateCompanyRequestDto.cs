using System.ComponentModel.DataAnnotations;

namespace StockPilot.Api.DTOs.Platform;

public sealed class CreateCompanyRequestDto
{
    [Required]
    [MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(256)]
    public string? CompanyEmail { get; set; }

    [MaxLength(30)]
    public string? CompanyPhone { get; set; }

    [Required]
    [MaxLength(200)]
    public string AdminFullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string AdminEmail { get; set; } = string.Empty;
}