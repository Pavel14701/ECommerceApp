using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;

public class ProductDeleteService : IProductDeleteService
{
    private readonly IDbContextFactory _dbContextFactory;
    private readonly string _uploadPath;

    public ProductDeleteService(IDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<ProductDeletionResultDto> DeleteProduct(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var product = await context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
        if (product != null)
        {
            foreach (var image in product.Images)
            {
                var filePath = Path.Combine(_uploadPath, image.ImageUrl);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            context.Products.Remove(product);
            await context.SaveChangesAsync();

            return new ProductDeletionResultDto
            {
                ProductId = id,
                Message = $"Product with ID: {id} has been deleted."
            };
        }

        return new ProductDeletionResultDto
        {
            ProductId = id,
            Message = $"Product with ID: {id} not found."
        };
    }

    public async Task<ImageDeletionResultDto> DeleteImage(Guid productId, Guid imageId)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var product = await context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null)
        {
            return new ImageDeletionResultDto
            {
                Success = false,
                Message = "Product not found."
            };
        }

        var image = product.Images.FirstOrDefault(i => i.Id == imageId);
        if (image == null)
        {
            return new ImageDeletionResultDto
            {
                Success = false,
                Message = "Image not found."
            };
        }

        var filePath = Path.Combine(_uploadPath, image.ImageUrl);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        product.Images.Remove(image);
        await context.SaveChangesAsync();

        return new ImageDeletionResultDto
        {
            Success = true,
            Message = $"Image with ID: {imageId} has been deleted from Product with ID: {productId}."
        };
    }
}
