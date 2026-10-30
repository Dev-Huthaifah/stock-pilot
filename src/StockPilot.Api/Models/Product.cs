namespace StockPilot.Api.Models;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CompanyId { get; set; }

    public string SKU { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Unit { get; set; } = "Piece";

    public decimal CostPrice { get; set; }

    public decimal SellingPrice { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Company Company { get; set; } = null!;

    public ICollection<ProductStock> ProductStocks { get; set; }
        = new List<ProductStock>();
}