using Microsoft.EntityFrameworkCore;
using Npgsql;

public class ProductDeleteService : IProductDeleteService
{
    private readonly SessionIterator _sessionIterator;
    private readonly string _uploadPath;

    public ProductDeleteService(SessionIterator sessionIterator)
    {
        _sessionIterator = sessionIterator;
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
        var product = await _sessionIterator.QueryAsync(async context =>
        {
            var commandText = "SELECT * FROM Products WHERE Id = @Id";
            return await context.Products.FromSqlRaw(commandText, new NpgsqlParameter("@Id", id))
                .Include(p => p.Images)
                .FirstOrDefaultAsync();
        });

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

            await _sessionIterator.ExecuteAsync(async context =>
            {
                var commandText = "DELETE FROM Products WHERE Id = @Id";
                await context.Database.ExecuteSqlRawAsync(commandText, new NpgsqlParameter("@Id", id));
            });

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
        var product = await _sessionIterator.QueryAsync(async context =>
        {
            var commandText = "SELECT * FROM Products WHERE Id = @ProductId";
            return await context.Products.FromSqlRaw(commandText, new NpgsqlParameter("@ProductId", productId))
                .Include(p => p.Images)
                .FirstOrDefaultAsync();
        });
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
        await _sessionIterator.ExecuteAsync(async context =>
        {
            var commandText = "DELETE FROM Images WHERE Id = @ImageId";
            await context.Database.ExecuteSqlRawAsync(commandText, new NpgsqlParameter("@ImageId", imageId));
        });
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
