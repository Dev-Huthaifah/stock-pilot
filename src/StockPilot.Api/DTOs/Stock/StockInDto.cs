using System.ComponentModel.DataAnnotations;

namespace StockPilot.Api.DTOs.Stock;

public class StockInDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public Guid WarehouseId { get; set; }

    [Range(typeof(decimal), "0.001", "999999999999999.999")]
    public decimal Quantity { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}