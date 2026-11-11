using Microsoft.AspNetCore.Mvc;
using StockPilot.Api.DTOs.Products;
using StockPilot.Api.Services.Interfaces;

namespace StockPilot.Api.Controllers;

[ApiController]
[Route("api/companies/{companyId:guid}/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        Guid companyId,
        [FromBody] CreateProductDto dto)
    {
        try
        {
            var product = await _productService.CreateAsync(companyId, dto);

            if (product == null)
                return NotFound(new
                {
                    message = "Company not found or inactive."
                });

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    companyId,
                    productId = product.Id
                },
                product);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        Guid companyId,
        [FromQuery] bool includeInactive = false)
    {
        var products = await _productService.GetAllAsync(
            companyId,
            includeInactive);

        return Ok(products);
    }

    [HttpGet("{productId:guid}")]
    public async Task<IActionResult> GetById(
        Guid companyId,
        Guid productId)
    {
        var product = await _productService.GetByIdAsync(
            companyId,
            productId);

        if (product == null)
            return NotFound(new { message = "Product not found." });

        return Ok(product);
    }

    [HttpPut("{productId:guid}")]
    public async Task<IActionResult> Update(
        Guid companyId,
        Guid productId,
        [FromBody] UpdateProductDto dto)
    {
        try
        {
            var product = await _productService.UpdateAsync(
                companyId,
                productId,
                dto);

            if (product == null)
                return NotFound(new { message = "Product not found." });

            return Ok(product);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{productId:guid}")]
    public async Task<IActionResult> Deactivate(
        Guid companyId,
        Guid productId)
    {
        var result = await _productService.DeactivateAsync(
            companyId,
            productId);

        if (!result)
            return NotFound(new { message = "Product not found." });

        return NoContent();
    }
}