using System.ComponentModel.DataAnnotations;

namespace StockPilot.Api.DTOs.StockTransfers;

public class CreateStockTransferDto
{
    [Required]
    public Guid SourceWarehouseId { get; set; }

    [Required]
    public Guid DestinationWarehouseId { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [Required]
    [MinLength(1)]
    public List<CreateStockTransferItemDto> Items { get; set; } = new();
}