using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
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

    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetProductsByCategory(string category, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var products = await _productService.GetProductsByCategory(category, pageNumber, pageSize);
        return Ok(products);
    }

    [HttpGet("search/{name}")]
    public async Task<IActionResult> GetProductsByName(string name, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var products = await _productService.GetProductsByName(name, pageNumber, pageSize);
        return Ok(products);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateProduct([FromBody] Product product)
    {
        await _productService.AddProduct(product);
        return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        await _productService.DeleteProduct(id);
        return NoContent();
    }

    [HttpPost("{id}/upload")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UploadImage(Guid id, [FromForm] IFormFile file)
    {
        var image = await _productService.UploadImage(id, file);
        if (image == null)
        {
            return BadRequest("Failed to upload image.");
        }
        return Ok(image);
    }

    [HttpDelete("{id}/images/{imageId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteImage(Guid id, Guid imageId)
    {
        var result = await _productService.DeleteImage(id, imageId);
        if (!result)
        {
            return BadRequest("Failed to delete image.");
        }
        return NoContent();
    }

    [HttpPut("{id}/images/{imageId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateImage(Guid id, Guid imageId, [FromForm] IFormFile file)
    {
        var image = await _productService.UpdateImage(id, imageId, file);
        if (image == null)
        {
            return BadRequest("Failed to update image.");
        }
        return Ok(image);
    }

    [HttpPut("{id}/name")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProductName(Guid id, [FromBody] string name)
    {
        await _productService.UpdateProductName(id, name);
        return NoContent();
    }

    [HttpPut("{id}/category")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProductCategory(Guid id, [FromBody] string category)
    {
        await _productService.UpdateProductCategory(id, category);
        return NoContent();
    }

    [HttpPut("{id}/price")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProductPrice(Guid id, [FromBody] decimal price)
    {
        await _productService.UpdateProductPrice(id, price);
        return NoContent();
    }

    [HttpPut("{id}/stock")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProductStock(Guid id, [FromBody] int stock)
    {
        await _productService.UpdateProductStock(id, stock);
        return NoContent();
    }

    [HttpPut("{id}/description")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProductDescription(Guid id, [FromBody] string description)
    {
        await _productService.UpdateProductDescription(id, description);
        return NoContent();
    }
}
