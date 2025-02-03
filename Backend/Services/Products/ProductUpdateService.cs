using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

public class ProductUpdateService : IProductUpdateService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly string _uploadPath;

    public ProductUpdateService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
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
        var commandText = @"
            UPDATE Products
            SET Name = @Name, SubcategoryId = @SubcategoryId, Price = @Price, Stock = @Stock, Description = @Description
            WHERE Id = @Id";

        await context.Database.ExecuteSqlRawAsync(commandText,
            new SqlParameter("@Id", product.Id),
            new SqlParameter("@Name", product.Name),
            new SqlParameter("@SubcategoryId", product.SubcategoryId),
            new SqlParameter("@Price", product.Price),
            new SqlParameter("@Stock", product.Stock),
            new SqlParameter("@Description", product.Description));

        return new ProductUpdateResultDto
        {
            ProductId = product.Id,
            Message = "Product has been updated."
        };
    }

    public async Task<ProductUpdateResultDto> UpdateProductName(Guid id, string name)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var commandText = "UPDATE Products SET Name = @Name WHERE Id = @Id";

        await context.Database.ExecuteSqlRawAsync(commandText,
            new SqlParameter("@Id", id),
            new SqlParameter("@Name", name));

        return new ProductUpdateResultDto
        {
            ProductId = id,
            Message = "Product name has been updated."
        };
    }

    public async Task<ProductUpdateResultDto> UpdateProductCategory(Guid id, Guid categoryId)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var commandText = "UPDATE Products SET CategoryId = @CategoryId WHERE Id = @Id";

        await context.Database.ExecuteSqlRawAsync(commandText,
            new SqlParameter("@Id", id),
            new SqlParameter("@CategoryId", categoryId));

        return new ProductUpdateResultDto
        {
            ProductId = id,
            Message = "Product category has been updated."
        };
    }

    public async Task<ProductUpdateResultDto> UpdateProductSubcategory(Guid id, Guid subcategoryId)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var commandText = "UPDATE Products SET SubcategoryId = @SubcategoryId WHERE Id = @Id";

        await context.Database.ExecuteSqlRawAsync(commandText,
            new SqlParameter("@Id", id),
            new SqlParameter("@SubcategoryId", subcategoryId));

        return new ProductUpdateResultDto
        {
            ProductId = id,
            Message = "Product subcategory has been updated."
        };
    }

    public async Task<ProductUpdateResultDto> UpdateProductPrice(Guid id, decimal price)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var commandText = "UPDATE Products SET Price = @Price WHERE Id = @Id";

        await context.Database.ExecuteSqlRawAsync(commandText,
            new SqlParameter("@Id", id),
            new SqlParameter("@Price", price));

        return new ProductUpdateResultDto
        {
            ProductId = id,
            Message = "Product price has been updated."
        };
    }

    public async Task<ProductUpdateResultDto> UpdateProductStock(Guid id, int stock)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var commandText = "UPDATE Products SET Stock = @Stock WHERE Id = @Id";

        await context.Database.ExecuteSqlRawAsync(commandText,
            new SqlParameter("@Id", id),
            new SqlParameter("@Stock", stock));

        return new ProductUpdateResultDto
        {
            ProductId = id,
            Message = "Product stock has been updated."
        };
    }

    public async Task<ProductUpdateResultDto> UpdateProductDescription(Guid id, string description)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var commandText = "UPDATE Products SET Description = @Description WHERE Id = @Id";

        await context.Database.ExecuteSqlRawAsync(commandText,
            new SqlParameter("@Id", id),
            new SqlParameter("@Description", description));

        return new ProductUpdateResultDto
        {
            ProductId = id,
            Message = "Product description has been updated."
        };
    }

    public async Task<ImageUpdateResultDto> UpdateProductImage(Guid productId, Guid imageId, IFormFile file)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var commandText = @"
            SELECT p.Id AS ProductId, i.Id AS ImageId, i.ImageUrl
            FROM Products p
            JOIN Images i ON p.Id = i.ProductId
            WHERE p.Id = @ProductId AND i.Id = @ImageId";

        var result = await context.Images
            .FromSqlRaw(commandText,
                new SqlParameter("@ProductId", productId),
                new SqlParameter("@ImageId", imageId))
            .ToListAsync();

        if (result.Count == 0 || file == null || file.Length == 0)
        {
            return new ImageUpdateResultDto
            {
                Message = "Invalid product ID or file."
            };
        }

        var image = result.First();
        var filePath = Path.Combine(_uploadPath, image.ImageUrl);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        var deleteCommandText = "DELETE FROM Images WHERE Id = @ImageId";
        await context.Database.ExecuteSqlRawAsync(deleteCommandText,
            new SqlParameter("@ImageId", imageId));

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var newFilePath = Path.Combine(_uploadPath, fileName);
        using (var stream = new FileStream(newFilePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var insertCommandText = @"
            INSERT INTO Images (Id, ImageUrl, ProductId) 
            VALUES (@Id, @ImageUrl, @ProductId)";
        var newImageId = Guid.NewGuid();
        await context.Database.ExecuteSqlRawAsync(insertCommandText,
            new SqlParameter("@Id", newImageId),
            new SqlParameter("@ImageUrl", fileName),
            new SqlParameter("@ProductId", productId));

        return new ImageUpdateResultDto
        {
            ImageId = newImageId,
            ImageUrl = fileName,
            Message = $"Image with ID: {imageId} has been updated for product with ID: {productId}."
        };
    }
}
