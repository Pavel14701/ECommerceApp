public class UpdateProductDto
{
    public Guid ProductId { get; set; }
    public string? Name { get; set; }
    public decimal? Price { get; set; }
    public int? Discount { get; set; }
    public Dictionary<Guid, Guid> CategorySubcategoryPairs { get; set; } = new Dictionary<Guid, Guid>(); // Словарь для категорий и подкатегорий
}


public class ProductUpdateService : IProductUpdateService
{
    private readonly SessionIterator _sessionIterator;
    private readonly UpdateCrud _updateCrud;

    public ProductUpdateService
    (
        SessionIterator sessionIterator,
        UpdateCrud updateCrud
    )
    {
        _sessionIterator = sessionIterator;
        _updateCrud = updateCrud;
    }

    public async Task<ProductUpdateResultDto> UpdateProduct(Product product)
    {
        var commandText = @"
            UPDATE Products
            SET Name = @Name, SubcategoryId = @SubcategoryId, Price = @Price, Stock = @Stock, Description = @Description
            WHERE Id = @Id";

        await _sessionIterator.ExecuteAsync(async context =>
        {
            await context.Database.ExecuteSqlRawAsync(commandText,
                new NpgsqlParameter("@Id", product.Id),
                new NpgsqlParameter("@Name", product.Name),
                new NpgsqlParameter("@SubcategoryId", product.SubcategoryId),
                new NpgsqlParameter("@Price", product.Price),
                new NpgsqlParameter("@Stock", product.Stock),
                new NpgsqlParameter("@Description", product.Description));
        });

        return new ProductUpdateResultDto
        {
            ProductId = product.Id,
            Message = "Product has been updated."
        };
    }

    public async Task<ProductUpdateResultDto> UpdateProductName(Guid id, string name)
    {
        var commandText = "UPDATE Products SET Name = @Name WHERE Id = @Id";

        await _sessionIterator.ExecuteAsync(async context =>
        {
            await context.Database.ExecuteSqlRawAsync(commandText,
                new NpgsqlParameter("@Id", id),
                new NpgsqlParameter("@Name", name));
        });

        return new ProductUpdateResultDto
        {
            ProductId = id,
            Message = "Product name has been updated."
        };
    }

    public async Task<ProductUpdateResultDto> UpdateProductCategory(Guid id, Guid categoryId)
    {
        var commandText = "UPDATE Products SET CategoryId = @CategoryId WHERE Id = @Id";

        await _sessionIterator.ExecuteAsync(async context =>
        {
            await context.Database.ExecuteSqlRawAsync(commandText,
                new NpgsqlParameter("@Id", id),
                new NpgsqlParameter("@CategoryId", categoryId));
        });

        return new ProductUpdateResultDto
        {
            ProductId = id,
            Message = "Product category has been updated."
        };
    }

    public async Task<ProductUpdateResultDto> UpdateProductSubcategory(Guid id, Guid subcategoryId)
    {
        var commandText = "UPDATE Products SET SubcategoryId = @SubcategoryId WHERE Id = @Id";

        await _sessionIterator.ExecuteAsync(async context =>
        {
            await context.Database.ExecuteSqlRawAsync(commandText,
                new NpgsqlParameter("@Id", id),
                new NpgsqlParameter("@SubcategoryId", subcategoryId));
        });

        return new ProductUpdateResultDto
        {
            ProductId = id,
            Message = "Product subcategory has been updated."
        };
    }

    public async Task<ProductUpdateResultDto> UpdateProductPrice(Guid id, decimal price)
    {
        var commandText = "UPDATE Products SET Price = @Price WHERE Id = @Id";

        await _sessionIterator.ExecuteAsync(async context =>
        {
            await context.Database.ExecuteSqlRawAsync(commandText,
                new NpgsqlParameter("@Id", id),
                new NpgsqlParameter("@Price", price));
        });

        return new ProductUpdateResultDto
        {
            ProductId = id,
            Message = "Product price has been updated."
        };
    }

    public async Task<ProductUpdateResultDto> UpdateProductStock(Guid id, int stock)
    {
        var commandText = "UPDATE Products SET Stock = @Stock WHERE Id = @Id";

        await _sessionIterator.ExecuteAsync(async context =>
        {
            await context.Database.ExecuteSqlRawAsync(commandText,
                new NpgsqlParameter("@Id", id),
                new NpgsqlParameter("@Stock", stock));
        });

        return new ProductUpdateResultDto
        {
            ProductId = id,
            Message = "Product stock has been updated."
        };
    }

    public async Task<ProductUpdateResultDto> UpdateProductDescription(Guid id, string description)
    {
        var commandText = "UPDATE Products SET Description = @Description WHERE Id = @Id";

        await _sessionIterator.ExecuteAsync(async context =>
        {
            await context.Database.ExecuteSqlRawAsync(commandText,
                new NpgsqlParameter("@Id", id),
                new NpgsqlParameter("@Description", description));
        });

        return new ProductUpdateResultDto
        {
            ProductId = id,
            Message = "Product description has been updated."
        };
    }

    public async Task<ImageUpdateResultDto> UpdateProductImage(Guid productId, Guid imageId, IFormFile file)
    {
        var commandText = @"
            SELECT p.Id AS ProductId, i.Id AS ImageId, i.ImageUrl
            FROM Products p
            JOIN Images i ON p.Id = i.ProductId
            WHERE p.Id = @ProductId AND i.Id = @ImageId";

        var result = await _sessionIterator.QueryAsync(async context =>
        {
            return await context.Images
                .FromSqlRaw(commandText,
                    new NpgsqlParameter("@ProductId", productId),
                    new NpgsqlParameter("@ImageId", imageId))
                .ToListAsync();
        });

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
        await _sessionIterator.ExecuteAsync(async context =>
        {
            await context.Database.ExecuteSqlRawAsync(deleteCommandText,
                new NpgsqlParameter("@ImageId", imageId));
        });

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
        await _sessionIterator.ExecuteAsync(async context =>
        {
            await context.Database.ExecuteSqlRawAsync(insertCommandText,
                new NpgsqlParameter("@Id", newImageId),
                new NpgsqlParameter("@ImageUrl", fileName),
                new NpgsqlParameter("@Product1Id", productId));
        });

        return new ImageUpdateResultDto
        {
            ImageId = newImageId,
            ImageUrl = fileName,
            Message = $"Image with ID: {imageId} has been updated for product with ID: {productId}."
        };
    }
}
