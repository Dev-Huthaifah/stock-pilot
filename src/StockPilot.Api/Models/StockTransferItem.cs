namespace StockPilot.Api.Models;

public class StockTransferItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CompanyId { get; set; }

    public Guid StockTransferId { get; set; }

    public Guid ProductId { get; set; }

    public decimal Quantity { get; set; }

    public StockTransfer StockTransfer { get; set; } = null!;

    public Product Product { get; set; } = null!;
}