using StockPilot.Api.DTOs.Products;

namespace StockPilot.Api.Services.Interfaces;

public interface IProductService
{
    Task<ProductResponseDto?> CreateAsync(
        Guid companyId,
        CreateProductDto dto);

    Task<List<ProductResponseDto>> GetAllAsync(
        Guid companyId,
        bool includeInactive = false);

    Task<ProductResponseDto?> GetByIdAsync(
        Guid companyId,
        Guid productId);

    Task<ProductResponseDto?> UpdateAsync(
        Guid companyId,
        Guid productId,
        UpdateProductDto dto);

    Task<bool> DeactivateAsync(
        Guid companyId,
        Guid productId);
}