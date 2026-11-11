using StockPilot.Api.Enums;

namespace StockPilot.Api.DTOs.Stock;

public class StockMovementResponseDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid? StockTransferItemId { get; set; }
    public StockMovementType MovementType { get; set; }
    public decimal Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public string? Notes { get; set; }
}