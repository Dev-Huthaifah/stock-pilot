using StockPilot.Api.DTOs.Stock;

namespace StockPilot.Api.Services.Interfaces;

public interface IStockService
{
    Task<StockResponseDto> StockInAsync(
        Guid companyId,
        StockInDto dto);

    Task<StockResponseDto> StockOutAsync(
        Guid companyId,
        StockOutDto dto);

    Task<StockResponseDto> AdjustAsync(
        Guid companyId,
        StockAdjustmentDto dto);

    Task<List<StockResponseDto>> GetAllAsync(
        Guid companyId,
        Guid? warehouseId = null,
        Guid? productId = null);

    Task<List<StockMovementResponseDto>> GetMovementsAsync(
        Guid companyId,
        Guid? warehouseId = null,
        Guid? productId = null);
}