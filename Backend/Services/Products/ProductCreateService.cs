using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;


public class ProductCreateService : IProductCreateService
{
    private readonly IDbContextFactory _dbContextFactory;
    private readonly string _uploadPath;

    public ProductCreateService(IDbContextFactory dbContextFactory)
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

    public async Task<ProductCreationResultDto> AddProduct(Product product)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var commandText = @"
            INSERT INTO Products (Id, Name, SubcategoryId, Price, Stock, Description)
            VALUES (@Id, @Name, @SubcategoryId, @Price, @Stock, @Description)
        ";
        await context.Database.ExecuteSqlRawAsync(commandText,
            new SqlParameter("@Id", product.Id),
            new SqlParameter("@Name", product.Name),
            new SqlParameter("@CategoryId", product.CategoryId),
            new SqlParameter("@SubcategoryId", product.SubcategoryId),
            new SqlParameter("@Price", product.Price),
            new SqlParameter("@Stock", product.Stock),
            new SqlParameter("@Description", product.Description));
        return new ProductCreationResultDto
        {
            ProductId = product.Id,
            Message = $"Product with ID: {product.Id} has been created."
        };
    }

    public async Task<ImageUploadResultDto> UploadImage(Guid productId, IFormFile file)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var productExists = await context.Products
            .FromSqlRaw(@"
                SELECT Id 
                FROM Products 
                WHERE Id = @ProductId
            ",
            new SqlParameter("@ProductId", productId))
            .AnyAsync();

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
        var insertImageCommand = @"
            INSERT INTO Images (Id, ImageUrl, ProductId)
            VALUES (@Id, @ImageUrl, @ProductId)
        ";
        await context.Database.ExecuteSqlRawAsync(insertImageCommand,
            new SqlParameter("@Id", imageId),
            new SqlParameter("@ImageUrl", fileName),
            new SqlParameter("@ProductId", productId));

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
