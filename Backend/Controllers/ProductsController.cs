using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var products = await _productService.GetAllProducts(pageNumber, pageSize);
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var product = await _productService.GetProductById(id);
        if (product == null)
        {
            return NotFound();
        }
        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] Product product)
    {
        await _productService.AddProduct(product);
        return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] Product product)
    {
        if (id != product.Id)
        {
            return BadRequest();
        }

        await _productService.UpdateProduct(product);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        await _productService.DeleteProduct(id);
        return NoContent();
    }

    [HttpPost("{id}/images")]
    public async Task<IActionResult> AddImageToProduct(Guid id, [FromBody] Images image)
    {
        await _productService.AddImageToProduct(id, image);
        return NoContent();
    }

    [HttpDelete("{id}/images/{imageId}")]
    public async Task<IActionResult> RemoveImageFromProduct(Guid id, Guid imageId)
    {
        await _productService.RemoveImageFromProduct(id, imageId);
        return NoContent();
    }

    [HttpPut("{id}/name")]
    public async Task<IActionResult> UpdateProductName(Guid id, [FromBody] string name)
    {
        await _productService.UpdateProductName(id, name);
        return NoContent();
    }

    [HttpPut("{id}/category")]
    public async Task<IActionResult> UpdateProductCategory(Guid id, [FromBody] string category)
    {
        await _productService.UpdateProductCategory(id, category);
        return NoContent();
    }

    [HttpPut("{id}/price")]
    public async Task<IActionResult> UpdateProductPrice(Guid id, [FromBody] decimal price)
    {
        await _productService.UpdateProductPrice(id, price);
        return NoContent();
    }

    [HttpPut("{id}/stock")]
    public async Task<IActionResult> UpdateProductStock(Guid id, [FromBody] int stock)
    {
        await _productService.UpdateProductStock(id, stock);
        return NoContent();
    }

    [HttpPut("{id}/description")]
    public async Task<IActionResult> UpdateProductDescription(Guid id, [FromBody] string description)
    {
        await _productService.UpdateProductDescription(id, description);
        return NoContent();
    }
}
