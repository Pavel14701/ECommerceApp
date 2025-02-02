using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class ProductCreateService : IProductCreateService
{
    private readonly IDbContextFactory _dbContextFactory;
    private readonly string _uploadPath;

    public ProductCreateService(IDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<ProductCreationResultDto> AddProduct(Product product)
    {
        using var context = _dbContextFactory.CreateDbContext();
        context.Products.Add(product);
        await context.SaveChangesAsync();
        
        return new ProductCreationResultDto
        {
            ProductId = product.Id,
            Message = $"Product with ID: {product.Id} has been created."
        };
    }

    public async Task<ImageUploadResultDto> UploadImage(Guid productId, IFormFile file)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var product = await context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null || file == null || file.Length == 0)
        {
            return new ImageUploadResultDto
            {
                Message = "Invalid product ID or file."
            };
        }

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(_uploadPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var image = new Images
        {
            Id = Guid.NewGuid(),
            ImageUrl = fileName,
            Alt = fileName
        };

        product.Images.Add(image);
        await context.SaveChangesAsync();

        return new ImageUploadResultDto
        {
            ImageId = image.Id,
            ImageUrl = fileName,
            Message = $"Image with ID: {image.Id} has been uploaded and added to Product with ID: {productId}."
        };
    }
}
