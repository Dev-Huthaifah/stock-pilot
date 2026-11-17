using StockPilot.Api.Enums;

namespace StockPilot.Api.DTOs.StockTransfers;

public class StockTransferResponseDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string TransferNumber { get; set; } = string.Empty;

    public Guid SourceWarehouseId { get; set; }
    public string SourceWarehouseName { get; set; } = string.Empty;

    public Guid DestinationWarehouseId { get; set; }
    public string DestinationWarehouseName { get; set; } = string.Empty;

    public StockTransferStatus Status { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? PostedAt { get; set; }

    public List<StockTransferItemResponseDto> Items { get; set; } = new();
}