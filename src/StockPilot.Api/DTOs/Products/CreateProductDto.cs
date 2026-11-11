using System.ComponentModel.DataAnnotations;

namespace StockPilot.Api.DTOs.Products;

public class CreateProductDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string SKU { get; set; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    [StringLength(30, MinimumLength = 1)]
    public string Unit { get; set; } = "Piece";

    [Range(typeof(decimal), "0", "9999999999999999.99")]
    public decimal CostPrice { get; set; }

    [Range(typeof(decimal), "0", "9999999999999999.99")]
    public decimal SellingPrice { get; set; }
}