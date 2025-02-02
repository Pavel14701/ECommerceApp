using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        var products = await context.Products
                                     .Include(p => p.Images)
                                     .Skip((pageNumber - 1) * pageSize)
                                     .Take(pageSize)
                                     .ToListAsync();

        var totalCount = await context.Products.CountAsync();
        var productDtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Category = p.Category,
            Price = p.Price,
            Stock = p.Stock,
            Description = p.Description,
            Images = p.Images.Select(i => new ImageDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                Alt = i.Alt
            })
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
        var products = await context.Products
                                     .Include(p => p.Images)
                                     .Where(p => p.Category == category)
                                     .Skip((pageNumber - 1) * pageSize)
                                     .Take(pageSize)
                                     .ToListAsync();

        var totalCount = await context.Products.CountAsync(p => p.Category == category);
        var productDtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Category = p.Category,
            Price = p.Price,
            Stock = p.Stock,
            Description = p.Description,
            Images = p.Images.Select(i => new ImageDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                Alt = i.Alt
            })
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
        var products = await context.Products
                                     .Include(p => p.Images)
                                     .Where(p => p.Name.Contains(name))
                                     .Skip((pageNumber - 1) * pageSize)
                                     .Take(pageSize)
                                     .ToListAsync();

        var totalCount = await context.Products.CountAsync(p => p.Name.Contains(name));
        var productDtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Category = p.Category,
            Price = p.Price,
            Stock = p.Stock,
            Description = p.Description,
            Images = p.Images.Select(i => new ImageDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                Alt = i.Alt
            })
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
        var product = await context.Products
                                    .Include(p => p.Images)
                                    .FirstOrDefaultAsync(p => p.Id == id);

        return product != null ? new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Category = product.Category,
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
