using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

[Route("api/[controller]")]
[ApiController]
public class productsController : ControllerBase
{
    private readonly IMessageSender _messageSender;

    public productsController(IMessageSender messageSender)
    {
        _messageSender = messageSender;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var queryId = Guid.NewGuid();
        var getAllProductsQuery = new GetAllProductsQuery
        {
            QueryId = queryId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _messageSender.SendCommandAndGetResponse<PagedProductsDto>("products.exchange", "products.getall", getAllProductsQuery);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var queryId = Guid.NewGuid();
        var getProductByIdQuery = new GetProductByIdQuery
        {
            QueryId = queryId,
            ProductId = id
        };

        var result = await _messageSender.SendCommandAndGetResponse<Product>("products.exchange", "products.getbyid", getProductByIdQuery);
        return result != null ? Ok(result) : NotFound(new { Message = "Product not found", ProductId = id });
    }

    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetProductsByCategory(string category, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var queryId = Guid.NewGuid();
        var getProductsByCategoryQuery = new GetProductsByCategoryQuery
        {
            QueryId = queryId,
            Category = category,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _messageSender.SendCommandAndGetResponse<PagedProductsDto>("products.exchange", "products.getbycategory", getProductsByCategoryQuery);
        return Ok(result);
    }

    [HttpGet("search/{name}")]
    public async Task<IActionResult> GetProductsByName(string name, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var queryId = Guid.NewGuid();
        var getProductsByNameQuery = new GetProductsByNameQuery
        {
            QueryId = queryId,
            Name = name,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _messageSender.SendCommandAndGetResponse<PagedProductsDto>("products.exchange", "products.getbyname", getProductsByNameQuery);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateProduct([FromBody] Product product)
    {
        var commandId = Guid.NewGuid();
        var createProductCommand = new CreateProductCommand
        {
            CommandId = commandId,
            Product = product
        };

        var result = await _messageSender.SendCommandAndGetResponse<ProductCreationResultDto>("products.exchange", "products.create", createProductCommand);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var commandId = Guid.NewGuid();
        var deleteProductCommand = new DeleteProductCommand
        {
            CommandId = commandId,
            ProductId = id
        };

        var result = await _messageSender.SendCommandAndGetResponse<ProductDeletionResultDto>("products.exchange", "products.delete", deleteProductCommand);
        return Ok(result);
    }

    [HttpDelete("{id}/images/{imageId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteImage(Guid id, Guid imageId)
    {
        var commandId = Guid.NewGuid();
        var deleteImageCommand = new DeleteImageCommand
        {
            CommandId = commandId,
            ObjectId = id,
            ImageId = imageId
        };

        var result = await _messageSender.SendCommandAndGetResponse<ImageDeletionResultDto>("products.exchange", "products.deleteimage", deleteImageCommand);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] Product product)
    {
        if (id != product.Id)
        {
            return BadRequest();
        }

        var commandId = Guid.NewGuid();
        var updateProductCommand = new UpdateProductCommand
        {
            CommandId = commandId,
            Product = product
        };

        var result = await _messageSender.SendCommandAndGetResponse<ProductUpdateResultDto>("products.exchange", "products.update", updateProductCommand);
        return Ok(result);
    }

    [HttpPut("{id}/name")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProductName(Guid id, [FromBody] string name)
    {
        var commandId = Guid.NewGuid();
        var updateProductNameCommand = new UpdateProductNameCommand
        {
            CommandId = commandId,
            ProductId = id,
            Name = name
        };

        var result = await _messageSender.SendCommandAndGetResponse<ProductUpdateResultDto>("products.exchange", "products.update.name", updateProductNameCommand);
        return Ok(result);
    }

    [HttpPut("{id}/category")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProductCategory(Guid id, [FromBody] Guid categoryId)
    {
        var commandId = Guid.NewGuid();
        var updateProductCategoryCommand = new UpdateProductCategoryCommand
        {
            CommandId = commandId,
            ProductId = id,
            CategoryId = categoryId
        };

        var result = await _messageSender.SendCommandAndGetResponse<ProductUpdateResultDto>("products.exchange", "products.update.category", updateProductCategoryCommand);
        return Ok(result);
    }

    [HttpPut("{id}/subcategory")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProductSubcategory(Guid id, [FromBody] Guid subcategoryId)
    {
        var commandId = Guid.NewGuid();
        var updateProductSubcategoryCommand = new UpdateProductSubcategoryCommand
        {
            CommandId = commandId,
            ProductId = id,
            SubcategoryId = subcategoryId
        };

        var result = await _messageSender.SendCommandAndGetResponse<ProductUpdateResultDto>("products.exchange", "products.update.subcategory", updateProductSubcategoryCommand);
        return Ok(result);
    }

    [HttpPut("{id}/price")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProductPrice(Guid id, [FromBody] decimal price)
    {
        var commandId = Guid.NewGuid();
        var updateProductPriceCommand = new UpdateProductPriceCommand
        {
            CommandId = commandId,
            ProductId = id,
            Price = price
        };

        var result = await _messageSender.SendCommandAndGetResponse<ProductUpdateResultDto>("products.exchange", "products.update.price", updateProductPriceCommand);
        return Ok(result);
    }

    [HttpPut("{id}/stock")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProductStock(Guid id, [FromBody] int stock)
    {
        var commandId = Guid.NewGuid();
        var updateProductStockCommand = new UpdateProductStockCommand
        {
            CommandId = commandId,
            ProductId = id,
            Stock = stock
        };

        var result = await _messageSender.SendCommandAndGetResponse<ProductUpdateResultDto>("products.exchange", "products.update.stock", updateProductStockCommand);
        return Ok(result);
    }

    [HttpPut("{id}/description")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProductDescription(Guid id, [FromBody] string description)
    {
        var commandId = Guid.NewGuid();
        var updateProductDescriptionCommand = new UpdateProductDescriptionCommand
        {
            CommandId = commandId,
            ProductId = id,
            Description = description
        };

        var result = await _messageSender.SendCommandAndGetResponse<ProductUpdateResultDto>("products.exchange", "products.update.description", updateProductDescriptionCommand);
        return Ok(result);
    }

    [HttpPut("{id}/images/{imageId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateImage(Guid id, Guid imageId, [FromForm] IFormFile file)
    {
        var commandId = Guid.NewGuid();
        var updateImageCommand = new UpdateProductImageCommand
        {
            CommandId = commandId,
            ProductId = id,
            ImageId = imageId,
            File = file
        };

        var result = await _messageSender.SendCommandAndGetResponse<ImageUpdateResultDto>("products.exchange", "products.update.image", updateImageCommand);
        return Ok(result);
    }
}
