using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

public class ProductReadService : IProductReadService
{
    private readonly IDbContextFactory _dbContextFactory;

    public ProductReadService(IDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<PagedProductsDto> GetAllProducts(int pageNumber, int pageSize)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var offset = (pageNumber - 1) * pageSize;

        var commandText = @"
            SELECT p.*, c.Name AS CategoryName, sc.Name AS SubcategoryName
            FROM Products p
            JOIN Subcategories sc ON p.SubcategoryId = sc.Id
            JOIN Categories c ON sc.CategoryId = c.Id
            ORDER BY p.Name
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY;
            SELECT COUNT(*) FROM Products;";
        
        var products = await context.Products
            .FromSqlRaw(commandText, 
                new SqlParameter("@Offset", offset), 
                new SqlParameter("@PageSize", pageSize))
            .Include(p => p.Images)
            .ToListAsync();
        
        var totalCount = await context.Products.CountAsync();

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

    public async Task<PagedProductsDto> GetProductsByCategory(string category, int pageNumber, int pageSize)
    {
        using var context = _dbContextFactory.CreateDbContext();
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
            SELECT COUNT(*) FROM Products p
            JOIN Subcategories sc ON p.SubcategoryId = sc.Id
            JOIN Categories c ON sc.CategoryId = c.Id
            WHERE c.Name = @Category;";

        var products = await context.Products
            .FromSqlRaw(commandText, 
                new SqlParameter("@Category", category), 
                new SqlParameter("@Offset", offset), 
                new SqlParameter("@PageSize", pageSize))
            .Include(p => p.Images)
            .ToListAsync();

        var totalCount = await context.Products
            .Join(context.Subcategories, p => p.SubcategoryId, sc => sc.Id, (p, sc) => new { p, sc })
            .Join(context.Categories, ps => ps.sc.CategoryId, c => c.Id, (ps, c) => new { ps.p, ps.sc, c })
            .CountAsync(p => p.c.Name == category);

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

    public async Task<PagedProductsDto> GetProductsBySubcategory(string subcategory, int pageNumber, int pageSize)
    {
        using var context = _dbContextFactory.CreateDbContext();
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
            SELECT COUNT(*) FROM Products p
            JOIN Subcategories sc ON p.SubcategoryId = sc.Id
            JOIN Categories c ON sc.CategoryId = c.Id
            WHERE c.Name = @Subcategory;";

        var products = await context.Products
            .FromSqlRaw(commandText, 
                new SqlParameter("@Subcategory", subcategory), 
                new SqlParameter("@Offset", offset), 
                new SqlParameter("@PageSize", pageSize))
            .Include(p => p.Images)
            .ToListAsync();

        var totalCount = await context.Products
            .Join(context.Subcategories, p => p.SubcategoryId, sc => sc.Id, (p, sc) => new { p, sc })
            .Join(context.Categories, ps => ps.sc.CategoryId, c => c.Id, (ps, c) => new { ps.p, ps.sc, c })
            .CountAsync(p => p.c.Name == subcategory);

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

    public async Task<PagedProductsDto> GetProductsByName(string name, int pageNumber, int pageSize)
    {
        using var context = _dbContextFactory.CreateDbContext();
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
            SELECT COUNT(*) FROM Products p
            JOIN Subcategories sc ON p.SubcategoryId = sc.Id
            JOIN Categories c ON sc.CategoryId = c.Id
            WHERE p.Name LIKE '%' + @Name + '%';";

        var products = await context.Products
            .FromSqlRaw(commandText, 
                new SqlParameter("@Name", name), 
                new SqlParameter("@Offset", offset), 
                new SqlParameter("@PageSize", pageSize))
            .Include(p => p.Images)
            .ToListAsync();

        var totalCount = await context.Products
            .Join(context.Subcategories, p => p.SubcategoryId, sc => sc.Id, (p, sc) => new { p, sc })
            .Join(context.Categories, ps => ps.sc.CategoryId, c => c.Id, (ps, c) => new { ps.p, ps.sc, c })
            .CountAsync(p => p.p.Name.Contains(name));

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
        using var context = _dbContextFactory.CreateDbContext();

        var commandText = @"
            SELECT p.*, c.Name AS CategoryName, sc.Name AS SubcategoryName
            FROM Products p
            JOIN Subcategories sc ON p.SubcategoryId = sc.Id
            JOIN Categories c ON sc.CategoryId = c.Id
            WHERE p.Id = @Id";

        var product = await context.Products
            .FromSqlRaw(commandText, new SqlParameter("@Id", id))
            .Include(p => p.Images)
            .FirstOrDefaultAsync();

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
