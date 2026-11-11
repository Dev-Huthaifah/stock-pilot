using StockPilot.Api.DTOs.Warehouses;

namespace StockPilot.Api.Services.Interfaces;

public interface IWarehouseService
{
    Task<WarehouseResponseDto?> CreateAsync(
        Guid companyId,
        CreateWarehouseDto dto);

    Task<List<WarehouseResponseDto>> GetAllAsync(
        Guid companyId);

    Task<WarehouseResponseDto?> GetByIdAsync(
        Guid companyId,
        Guid warehouseId);

    Task<WarehouseResponseDto?> UpdateAsync(
        Guid companyId,
        Guid warehouseId,
        UpdateWarehouseDto dto);

    Task<bool> ActivateAsync(
        Guid companyId,
        Guid warehouseId);

    Task<bool> DeactivateAsync(
        Guid companyId,
        Guid warehouseId);
}