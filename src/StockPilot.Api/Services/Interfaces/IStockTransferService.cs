using StockPilot.Api.DTOs.StockTransfers;

namespace StockPilot.Api.Services.Interfaces;

public interface IStockTransferService
{
    Task<StockTransferResponseDto> CreateAsync(
        Guid companyId,
        CreateStockTransferDto dto);

    Task<List<StockTransferResponseDto>> GetAllAsync(
    Guid companyId,
    Guid? warehouseId = null);

    Task<StockTransferResponseDto> GetByIdAsync(
        Guid companyId,
        Guid transferId,
        Guid? warehouseId = null);

    Task<StockTransferResponseDto> PostAsync(
        Guid companyId,
        Guid transferId);

    Task CancelAsync(
        Guid companyId,
        Guid transferId);
}