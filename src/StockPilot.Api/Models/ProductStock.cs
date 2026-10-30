namespace StockPilot.Api.Models;

public class ProductStock
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CompanyId { get; set; }

    public Guid ProductId { get; set; }

    public Guid WarehouseId { get; set; }

    public decimal Quantity { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public byte[] RowVersion { get; set; } = [];

    public Product Product { get; set; } = null!;

    public Warehouse Warehouse { get; set; } = null!;
}