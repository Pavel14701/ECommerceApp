using Microsoft.EntityFrameworkCore;
using Npgsql;

public class ProductReadService : IProductReadService
{
    private readonly SessionIterator _sessionIterator;
    public ProductReadService(SessionIterator sessionIterator)
    {
        _sessionIterator = sessionIterator;
    }

    public async Task<PagedProductsDto> GetAllProducts(int pageNumber, int pageSize)
    {
        var offset = (pageNumber - 1) * pageSize;
        var commandText = @"
            SELECT p.*, c.Name AS CategoryName, sc.Name AS SubcategoryName
            FROM Products p
            JOIN Subcategories sc ON p.SubcategoryId = sc.Id
            JOIN Categories c ON sc.CategoryId = c.Id
            ORDER BY p.Name
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY;
        ";
        var countText = "SELECT COUNT(*) FROM Products;";
        var products = await _sessionIterator.QueryAsync(async context =>
        {
            return await context.Products
                .FromSqlRaw(commandText, 
                    new NpgsqlParameter("@Offset", offset), 
                    new NpgsqlParameter("@PageSize", pageSize))
                .Include(p => p.Images)
                .ToListAsync();
        });
        var totalCount = await _sessionIterator.ExecuteScalarAsync(countText);
        var productDtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Category = p.Subcategory?.Category?.Name,
            Subcategory = p.Subcategory?.Name,
            Price = p.Price,
            Stock = p.Stock,
            Description = p.Description,
            Images = p.Images.Select(i => new ImageDto 
            { 
                Id = i.Id, 
                ImageUrl = i.ImageUrl, 
                Alt = i.Alt 
            }).ToList()
        }).ToList();
        return new PagedProductsDto
        {
            TotalCount = totalCount,
            Products = productDtos
        };
    }

    public async Task<PagedProductsDto> GetProductsByCategory(
        string category, int pageNumber, int pageSize
    )
    {
        var offset = (pageNumber - 1) * pageSize;
        var commandText = @"
            SELECT p.*, c.Name AS CategoryName, sc.Name AS SubcategoryName
            FROM Products p
            JOIN Subcategories sc ON p.SubcategoryId = sc.Id
            JOIN Categories c ON sc.CategoryId = c.Id
            WHERE c.Name = @Category
            ORDER BY p.Name
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY;
        ";
        
        var countText = @"
            SELECT COUNT(*) FROM Products p
            JOIN Subcategories sc ON p.SubcategoryId = sc.Id
            JOIN Categories c ON sc.CategoryId = c.Id
            WHERE c.Name = @Category;
        ";

        var products = await _sessionIterator.QueryAsync(async context =>
        {
            return await context.Products
                .FromSqlRaw(commandText, 
                    new NpgsqlParameter("@Category", category), 
                    new NpgsqlParameter("@Offset", offset), 
                    new NpgsqlParameter("@PageSize", pageSize))
                .Include(p => p.Images)
                .ToListAsync();
        });
        
        var totalCount = await _sessionIterator.ExecuteScalarAsync(countText, new NpgsqlParameter("@Category", category));

        var productDtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Category = p.Subcategory?.Category?.Name,
            Subcategory = p.Subcategory?.Name,
            Price = p.Price,
            Stock = p.Stock,
            Description = p.Description,
            Images = p.Images.Select(i => new ImageDto 
            { 
                Id = i.Id, 
                ImageUrl = i.ImageUrl, 
                Alt = i.Alt 
            }).ToList()
        }).ToList();

        return new PagedProductsDto
        {
            TotalCount = totalCount,
            Products = productDtos
        };
    }

    public async Task<PagedProductsDto> GetProductsBySubcategory(
        string subcategory, int pageNumber, int pageSize
    )
    {
        var offset = (pageNumber - 1) * pageSize;
        var commandText = @"
            SELECT p.*, c.Name AS CategoryName, sc.Name AS SubcategoryName
            FROM Products p
            JOIN Subcategories sc ON p.SubcategoryId = sc.Id
            JOIN Categories c ON sc.CategoryId = c.Id
            WHERE sc.Name = @Subcategory
            ORDER BY p.Name
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY;
        ";
        var countText = @"
            SELECT COUNT(*) FROM Products p
            JOIN Subcategories sc ON p.SubcategoryId = sc.Id
            JOIN Categories c ON sc.CategoryId = c.Id
            WHERE sc.Name = @Subcategory;
        ";
        var products = await _sessionIterator.QueryAsync(async context =>
        {
            return await context.Products
                .FromSqlRaw(commandText, 
                    new NpgsqlParameter("@Subcategory", subcategory), 
                    new NpgsqlParameter("@Offset", offset), 
                    new NpgsqlParameter("@PageSize", pageSize))
                .Include(p => p.Images)
                .ToListAsync();
        });
        var totalCount = await _sessionIterator.ExecuteScalarAsync(countText, new NpgsqlParameter("@Subcategory", subcategory));
        var productDtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Category = p.Subcategory?.Category?.Name,
            Subcategory = p.Subcategory?.Name,
            Price = p.Price,
            Stock = p.Stock,
            Description = p.Description,
            Images = p.Images.Select(i => new ImageDto 
            { 
                Id = i.Id, 
                ImageUrl = i.ImageUrl, 
                Alt = i.Alt 
            }).ToList()
        }).ToList();
        return new PagedProductsDto
        {
            TotalCount = totalCount,
            Products = productDtos
        };
    }

    public async Task<PagedProductsDto> GetProductsByName(
        string name, int pageNumber, int pageSize
    )
    {
        var offset = (pageNumber - 1) * pageSize;
        var commandText = @"
            SELECT p.*, c.Name AS CategoryName, sc.Name AS SubcategoryName
            FROM Products p
            JOIN Subcategories sc ON p.SubcategoryId = sc.Id
            JOIN Categories c ON sc.CategoryId = c.Id
            WHERE p.Name LIKE '%' + @Name + '%'
            ORDER BY p.Name
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY;
        ";
        var countText = @"
            SELECT COUNT(*) FROM Products p
            JOIN Subcategories sc ON p.SubcategoryId = sc.Id
            JOIN Categories c ON sc.CategoryId = c.Id
            WHERE p.Name LIKE '%' + @Name + '%';
        ";
        var products = await _sessionIterator.QueryAsync(async context =>
        {
            return await context.Products
                .FromSqlRaw(commandText, 
                    new NpgsqlParameter("@Name", name), 
                    new NpgsqlParameter("@Offset", offset), 
                    new NpgsqlParameter("@PageSize", pageSize))
                .Include(p => p.Images)
                .ToListAsync();
        });
        var totalCount = await _sessionIterator.ExecuteScalarAsync(countText, new NpgsqlParameter("@Name", name));
        var productDtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Category = p.Subcategory?.Category?.Name,
            Subcategory = p.Subcategory?.Name,
            Price = p.Price,
            Stock = p.Stock,
            Description = p.Description,
            Images = p.Images.Select(i => new ImageDto 
            { 
                Id = i.Id, 
                ImageUrl = i.ImageUrl, 
                Alt = i.Alt 
            }).ToList()
        }).ToList();
        return new PagedProductsDto
        {
            TotalCount = totalCount,
            Products = productDtos
        };
    }

    public async Task<ProductDto> GetProductById(Guid id)
    {
        var commandText = @"
            SELECT p.*, c.Name AS CategoryName, sc.Name AS SubcategoryName
            FROM Products p
            JOIN Subcategories sc ON p.SubcategoryId = sc.Id
            JOIN Categories c ON sc.CategoryId = c.Id
            WHERE p.Id = @Id
        ";
        var product = await _sessionIterator.QueryAsync(async context =>
        {
            return await context.Products
                .FromSqlRaw(commandText, new NpgsqlParameter("@Id", id))
                .Include(p => p.Images)
                .FirstOrDefaultAsync();
        });
        return product != null ? new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Category = product.Subcategory?.Category?.Name,
            Subcategory = product.Subcategory?.Name,
            Price = product.Price,
            Stock = product.Stock,
            Description = product.Description,
            Images = product.Images.Select(i => new ImageDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                Alt = i.Alt
            }).ToList()
        } : new ProductDto();
    }
}
