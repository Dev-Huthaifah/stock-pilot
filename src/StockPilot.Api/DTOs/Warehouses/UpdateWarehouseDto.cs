using System.ComponentModel.DataAnnotations;

namespace StockPilot.Api.DTOs.Warehouses;

public class UpdateWarehouseDto
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Code { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Address { get; set; }
}