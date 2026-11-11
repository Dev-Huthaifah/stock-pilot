namespace StockPilot.Api.DTOs.Stock;

public class StockResponseDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal Quantity { get; set; }
    public DateTime UpdatedAt { get; set; }
}