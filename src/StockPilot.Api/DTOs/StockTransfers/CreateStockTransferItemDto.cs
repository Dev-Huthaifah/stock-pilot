using System.ComponentModel.DataAnnotations;

namespace StockPilot.Api.DTOs.StockTransfers;

public class CreateStockTransferItemDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(typeof(decimal), "0.001", "999999999999999.999")]
    public decimal Quantity { get; set; }
}