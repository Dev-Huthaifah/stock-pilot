using StockPilot.Api.Enums;

namespace StockPilot.Api.Models;

public class StockTransfer
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CompanyId { get; set; }

    public string TransferNumber { get; set; } = string.Empty;

    public Guid SourceWarehouseId { get; set; }

    public Guid DestinationWarehouseId { get; set; }

    public StockTransferStatus Status { get; set; }
        = StockTransferStatus.Draft;

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? PostedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Warehouse SourceWarehouse { get; set; } = null!;

    public Warehouse DestinationWarehouse { get; set; } = null!;

    public ICollection<StockTransferItem> Items { get; set; }
        = new List<StockTransferItem>();
}