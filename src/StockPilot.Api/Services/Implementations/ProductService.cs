using Microsoft.EntityFrameworkCore;
using StockPilot.Api.Data;
using StockPilot.Api.DTOs.Products;
using StockPilot.Api.Models;
using StockPilot.Api.Services.Interfaces;

namespace StockPilot.Api.Services.Implementations;

public class ProductService : IProductService
{
    private readonly StockPilotDbContext _context;

    public ProductService(StockPilotDbContext context)
    {
        _context = context;
    }

    public async Task<ProductResponseDto?> CreateAsync(
        Guid companyId,
        CreateProductDto dto)
    {
        var companyExists = await _context.Companies
            .AnyAsync(x => x.Id == companyId && x.IsActive);

        if (!companyExists)
            return null;

        var sku = dto.SKU.Trim();

        var skuExists = await _context.Products
            .AnyAsync(x => x.CompanyId == companyId && x.SKU == sku);

        if (skuExists)
            throw new InvalidOperationException(
                "A product with this SKU already exists.");

        var product = new Product
        {
            CompanyId = companyId,
            SKU = sku,
            Name = dto.Name.Trim(),
            Description = NormalizeDescription(dto.Description),
            Unit = dto.Unit.Trim(),
            CostPrice = dto.CostPrice,
            SellingPrice = dto.SellingPrice,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return MapToDto(product);
    }

    public async Task<List<ProductResponseDto>> GetAllAsync(
        Guid companyId,
        bool includeInactive = false)
    {
        var query = _context.Products
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId);

        if (!includeInactive)
            query = query.Where(x => x.IsActive);

        var products = await query
            .OrderBy(x => x.Name)
            .ToListAsync();

        return products.Select(MapToDto).ToList();
    }

    public async Task<ProductResponseDto?> GetByIdAsync(
        Guid companyId,
        Guid productId)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.CompanyId == companyId &&
                x.Id == productId);

        return product == null ? null : MapToDto(product);
    }

    public async Task<ProductResponseDto?> UpdateAsync(
        Guid companyId,
        Guid productId,
        UpdateProductDto dto)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(x =>
                x.CompanyId == companyId &&
                x.Id == productId);

        if (product == null)
            return null;

        var sku = dto.SKU.Trim();

        var skuExists = await _context.Products.AnyAsync(x =>
            x.CompanyId == companyId &&
            x.SKU == sku &&
            x.Id != productId);

        if (skuExists)
            throw new InvalidOperationException(
                "A product with this SKU already exists.");

        product.SKU = sku;
        product.Name = dto.Name.Trim();
        product.Description = NormalizeDescription(dto.Description);
        product.Unit = dto.Unit.Trim();
        product.CostPrice = dto.CostPrice;
        product.SellingPrice = dto.SellingPrice;

        await _context.SaveChangesAsync();

        return MapToDto(product);
    }

    public async Task<bool> DeactivateAsync(
        Guid companyId,
        Guid productId)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(x =>
                x.CompanyId == companyId &&
                x.Id == productId);

        if (product == null)
            return false;

        if (!product.IsActive)
            return true;

        product.IsActive = false;

        await _context.SaveChangesAsync();

        return true;
    }

    private static string? NormalizeDescription(string? description)
    {
        return string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
    }

    private static ProductResponseDto MapToDto(Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            CompanyId = product.CompanyId,
            SKU = product.SKU,
            Name = product.Name,
            Description = product.Description,
            Unit = product.Unit,
            CostPrice = product.CostPrice,
            SellingPrice = product.SellingPrice,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt
        };
    }
}