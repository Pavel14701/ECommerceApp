using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;

public class ProductDeleteService : IProductDeleteService
{
    private readonly ApplicationDbContext _context;
    private readonly string _uploadPath;

    public ProductDeleteService(ApplicationDbContext context)
    {
        _context = context;
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<ProductDeletionResultDto> DeleteProduct(Guid id)
    {
        var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
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

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

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
        var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == productId);
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
        await _context.SaveChangesAsync();

        return new ImageDeletionResultDto
        {
            Success = true,
            Message = $"Image with ID: {imageId} has been deleted from Product with ID: {productId}."
        };
    }
}
