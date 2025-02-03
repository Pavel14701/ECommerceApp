using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

public class ProductDeleteService : IProductDeleteService
{
    private readonly IDbContextFactory _dbContextFactory;
    private readonly string _uploadPath;

    public ProductDeleteService(IDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
        _uploadPath = Path.Combine(
            Directory.GetCurrentDirectory(), "wwwroot", "uploads"
        );
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<ProductDeletionResultDto> DeleteProduct(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var commandText = "SELECT * FROM Products WHERE Id = @Id";
        var product = await context.Products.FromSqlRaw(
            commandText, new SqlParameter("@Id", id)
        ).Include(p => p.Images).FirstOrDefaultAsync();
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
            commandText = "DELETE FROM Products WHERE Id = @Id";
            await context.Database.ExecuteSqlRawAsync(
                commandText, new SqlParameter("@Id", id)
            );
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

        var commandText = "SELECT * FROM Products WHERE Id = @ProductId";
        var product = await context.Products.FromSqlRaw(
            commandText, new SqlParameter("@ProductId", productId)
        ).Include(p => p.Images).FirstOrDefaultAsync();
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
        commandText = "DELETE FROM Images WHERE Id = @ImageId";
        await context.Database.ExecuteSqlRawAsync(
            commandText, new SqlParameter("@ImageId", imageId)
        );
        return new ImageDeletionResultDto
        {
            Success = true,
            Message = $@"
                Image with ID:{imageId} has been deleted 
                from Product with ID: {productId}.
            "
        };
    }
}
