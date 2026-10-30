using StockPilot.Api.Enums;

namespace StockPilot.Api.Models;

public class StockMovement
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CompanyId { get; set; }

    public Guid ProductId { get; set; }

    public Guid WarehouseId { get; set; }

    public Guid? StockTransferItemId { get; set; }

    public StockMovementType MovementType { get; set; }

    public decimal Quantity { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid? CreatedBy { get; set; }

    public string? Notes { get; set; }

    public Product Product { get; set; } = null!;

    public Warehouse Warehouse { get; set; } = null!;

    public StockTransferItem? StockTransferItem { get; set; }
}