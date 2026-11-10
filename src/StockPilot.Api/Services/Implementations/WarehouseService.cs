using Microsoft.EntityFrameworkCore;
using StockPilot.Api.Data;
using StockPilot.Api.DTOs.Warehouses;
using StockPilot.Api.Models;
using StockPilot.Api.Services.Interfaces;

namespace StockPilot.Api.Services.Implementations;

public class WarehouseService : IWarehouseService
{
    private readonly StockPilotDbContext _context;

    public WarehouseService(StockPilotDbContext context)
    {
        _context = context;
    }

    public async Task<WarehouseResponseDto?> CreateAsync(
        Guid companyId,
        CreateWarehouseDto dto)
    {
        var companyExists = await _context.Companies
            .AnyAsync(c => c.Id == companyId && c.IsActive);

        if (!companyExists)
            return null;

        var code = dto.Code.Trim();

        var codeExists = await _context.Warehouses
            .AnyAsync(w =>
                w.CompanyId == companyId &&
                w.Code == code);

        if (codeExists)
            throw new InvalidOperationException(
                "A warehouse with this code already exists.");

        var warehouse = new Warehouse
        {
            CompanyId = companyId,
            Name = dto.Name.Trim(),
            Code = code,
            Address = NormalizeAddress(dto.Address),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Warehouses.Add(warehouse);

        await _context.SaveChangesAsync();

        return MapToResponse(warehouse);
    }

    public async Task<List<WarehouseResponseDto>> GetAllAsync(
        Guid companyId)
    {
        var warehouses = await _context.Warehouses
            .AsNoTracking()
            .Where(w => w.CompanyId == companyId)
            .OrderBy(w => w.Name)
            .ToListAsync();

        return warehouses
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<WarehouseResponseDto?> GetByIdAsync(
        Guid companyId,
        Guid warehouseId)
    {
        var warehouse = await _context.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w =>
                w.Id == warehouseId &&
                w.CompanyId == companyId);

        return warehouse is null
            ? null
            : MapToResponse(warehouse);
    }

    public async Task<WarehouseResponseDto?> UpdateAsync(
        Guid companyId,
        Guid warehouseId,
        UpdateWarehouseDto dto)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w =>
                w.Id == warehouseId &&
                w.CompanyId == companyId);

        if (warehouse is null)
            return null;

        var code = dto.Code.Trim();

        var codeExists = await _context.Warehouses
            .AnyAsync(w =>
                w.CompanyId == companyId &&
                w.Code == code &&
                w.Id != warehouseId);

        if (codeExists)
            throw new InvalidOperationException(
                "A warehouse with this code already exists.");

        warehouse.Name = dto.Name.Trim();
        warehouse.Code = code;
        warehouse.Address = NormalizeAddress(dto.Address);

        await _context.SaveChangesAsync();

        return MapToResponse(warehouse);
    }

    public async Task<bool> DeactivateAsync(
        Guid companyId,
        Guid warehouseId)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w =>
                w.Id == warehouseId &&
                w.CompanyId == companyId);

        if (warehouse is null)
            return false;

        if (!warehouse.IsActive)
            return true;

        warehouse.IsActive = false;

        await _context.SaveChangesAsync();

        return true;
    }

    private static string? NormalizeAddress(string? address)
    {
        return string.IsNullOrWhiteSpace(address)
            ? null
            : address.Trim();
    }

    private static WarehouseResponseDto MapToResponse(
        Warehouse warehouse)
    {
        return new WarehouseResponseDto
        {
            Id = warehouse.Id,
            CompanyId = warehouse.CompanyId,
            Name = warehouse.Name,
            Code = warehouse.Code,
            Address = warehouse.Address,
            IsActive = warehouse.IsActive,
            CreatedAt = warehouse.CreatedAt
        };
    }
}