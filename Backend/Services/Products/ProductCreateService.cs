using Microsoft.EntityFrameworkCore;
using Npgsql;

public class ProductCreateService : IProductCreateService
{
    private readonly SessionIterator _sessionIterator;
    private readonly string _uploadPath;

    public ProductCreateService(SessionIterator sessionIterator)
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

    public async Task<ProductCreationResultDto> AddProduct(Product product)
    {
        await _sessionIterator.ExecuteAsync(async context =>
        {
            var commandText = @"
                INSERT INTO Products (Id, Name, SubcategoryId, Price, Stock, Description)
                VALUES (@Id, @Name, @SubcategoryId, @Price, @Stock, @Description)
            ";
            await context.Database.ExecuteSqlRawAsync(commandText,
                new NpgsqlParameter("@Id", product.Id),
                new NpgsqlParameter("@Name", product.Name),
                new NpgsqlParameter("@CategoryId", product.CategoryId),
                new NpgsqlParameter("@SubcategoryId", product.SubcategoryId),
                new NpgsqlParameter("@Price", product.Price),
                new NpgsqlParameter("@Stock", product.Stock),
                new NpgsqlParameter("@Description", product.Description));
        });

        return new ProductCreationResultDto
        {
            ProductId = product.Id,
            Message = $"Product with ID: {product.Id} has been created."
        };
    }

    public async Task<ImageUploadResultDto> UploadImage(Guid productId, IFormFile file)
    {
        var productExists = await _sessionIterator.QueryAsync(async context =>
        {
            return await context.Products
                .FromSqlRaw(@"
                    SELECT Id 
                    FROM Products 
                    WHERE Id = @ProductId
                ",
                new NpgsqlParameter("@ProductId", productId))
                .AnyAsync();
        });

        if (!productExists || file == null || file.Length == 0)
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

        var imageId = Guid.NewGuid();
        await _sessionIterator.ExecuteAsync(async context =>
        {
            var insertImageCommand = @"
                INSERT INTO Images (Id, ImageUrl, ProductId)
                VALUES (@Id, @ImageUrl, @ProductId)
            ";
            await context.Database.ExecuteSqlRawAsync(insertImageCommand,
                new NpgsqlParameter("@Id", imageId),
                new NpgsqlParameter("@ImageUrl", fileName),
                new NpgsqlParameter("@ProductId", productId));
        });

        return new ImageUploadResultDto
        {
            ImageId = imageId,
            ImageUrl = fileName,
            Message = $@"
                Image with ID: {imageId} has been uploaded and
                added to Product with ID: {productId}.
            "
        };
    }
}
