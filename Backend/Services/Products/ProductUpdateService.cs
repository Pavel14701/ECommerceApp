using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class ProductUpdateService : IProductUpdateService
{
    private readonly IDbContextFactory _dbContextFactory;
    private readonly string _uploadPath;

    public ProductUpdateService(IDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<ProductUpdateResultDto> UpdateProduct(Product product)
    {
        using var context = _dbContextFactory.CreateDbContext();
        context.Products.Update(product);
        await context.SaveChangesAsync();

        return new ProductUpdateResultDto
        {
            ProductId = product.Id,
            Message = $"Product with ID: {product.Id} has been updated."
        };
    }

    public async Task<ProductUpdateResultDto> UpdateProductName(Guid id, string name)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var product = await context.Products.FindAsync(id);
        if (product != null)
        {
            product.Name = name;
            await context.SaveChangesAsync();

            return new ProductUpdateResultDto
            {
                ProductId = id,
                Message = $"Product name updated to '{name}' for product with ID: {id}."
            };
        }

        return new ProductUpdateResultDto
        {
            ProductId = id,
            Message = $"Product with ID: {id} not found."
        };
    }

    public async Task<ProductUpdateResultDto> UpdateProductCategory(Guid id, string category)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var product = await context.Products.FindAsync(id);
        if (product != null)
        {
            product.Category = category;
            await context.SaveChangesAsync();

            return new ProductUpdateResultDto
            {
                ProductId = id,
                Message = $"Product category updated to '{category}' for product with ID: {id}."
            };
        }

        return new ProductUpdateResultDto
        {
            ProductId = id,
            Message = $"Product with ID: {id} not found."
        };
    }

    public async Task<ProductUpdateResultDto> UpdateProductPrice(Guid id, decimal price)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var product = await context.Products.FindAsync(id);
        if (product != null)
        {
            product.Price = price;
            await context.SaveChangesAsync();

            return new ProductUpdateResultDto
            {
                ProductId = id,
                Message = $"Product price updated to '{price}' for product with ID: {id}."
            };
        }

        return new ProductUpdateResultDto
        {
            ProductId = id,
            Message = $"Product with ID: {id} not found."
        };
    }

    public async Task<ProductUpdateResultDto> UpdateProductStock(Guid id, int stock)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var product = await context.Products.FindAsync(id);
        if (product != null)
        {
            product.Stock = stock;
            await context.SaveChangesAsync();

            return new ProductUpdateResultDto
            {
                ProductId = id,
                Message = $"Product stock updated to '{stock}' for product with ID: {id}."
            };
        }

        return new ProductUpdateResultDto
        {
            ProductId = id,
            Message = $"Product with ID: {id} not found."
        };
    }

    public async Task<ProductUpdateResultDto> UpdateProductDescription(Guid id, string description)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var product = await context.Products.FindAsync(id);
        if (product != null)
        {
            product.Description = description;
            await context.SaveChangesAsync();

            return new ProductUpdateResultDto
            {
                ProductId = id,
                Message = $"Product description updated for product with ID: {id}."
            };
        }

        return new ProductUpdateResultDto
        {
            ProductId = id,
            Message = $"Product with ID: {id} not found."
        };
    }

    public async Task<ImageUpdateResultDto> UpdateProductImage(Guid productId, Guid imageId, IFormFile file)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var product = await context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null || file == null || file.Length == 0)
        {
            return new ImageUpdateResultDto
            {
                Message = "Invalid product ID or file."
            };
        }

        var image = product.Images.FirstOrDefault(i => i.Id == imageId);
        if (image != null)
        {
            var filePath = Path.Combine(_uploadPath, image.ImageUrl);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            product.Images.Remove(image);
        }

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var newFilePath = Path.Combine(_uploadPath, fileName);

        using (var stream = new FileStream(newFilePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var newImage = new Images
        {
            Id = Guid.NewGuid(),
            ImageUrl = fileName,
            Alt = fileName
        };

        product.Images.Add(newImage);
        await context.SaveChangesAsync();

        return new ImageUpdateResultDto
        {
            ImageId = newImage.Id,
            ImageUrl = fileName,
            Message = $"Image with ID: {imageId} has been updated for product with ID: {productId}."
        };
    }
}
