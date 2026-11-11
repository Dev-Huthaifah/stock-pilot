using System.ComponentModel.DataAnnotations;

namespace StockPilot.Api.DTOs.Stock;

public class StockAdjustmentDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public Guid WarehouseId { get; set; }

    // Positive = increase, Negative = decrease.
    [Range(typeof(decimal), "-999999999999999.999", "999999999999999.999")]
    public decimal Difference { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}