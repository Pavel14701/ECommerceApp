using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;
    private readonly string _uploadPath;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<IEnumerable<Product>> GetAllProducts(int pageNumber, int pageSize)
    {
        return await _context.Products
                             .Include(p => p.Images)
                             .Skip((pageNumber - 1) * pageSize)
                             .Take(pageSize)
                             .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetProductsByCategory(string category, int pageNumber, int pageSize)
    {
        return await _context.Products
                             .Include(p => p.Images)
                             .Where(p => p.Category == category)
                             .Skip((pageNumber - 1) * pageSize)
                             .Take(pageSize)
                             .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetProductsByName(string name, int pageNumber, int pageSize)
    {
        return await _context.Products
                             .Include(p => p.Images)
                             .Where(p => p.Name.Contains(name))
                             .Skip((pageNumber - 1) * pageSize)
                             .Take(pageSize)
                             .ToListAsync();
    }

    public async Task<Product?> GetProductById(Guid id)
    {
        return await _context.Products
                             .Include(p => p.Images)
                             .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddProduct(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateProduct(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteProduct(Guid id)
    {
        var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
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

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddImageToProduct(Guid productId, Images image)
    {
        var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == productId);
        if (product != null)
        {
            product.Images.Add(image);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveImageFromProduct(Guid productId, Guid imageId)
    {
        var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == productId);
        if (product != null)
        {
            var image = product.Images.FirstOrDefault(i => i.Id == imageId);
            if (image != null)
            {
                var filePath = Path.Combine(_uploadPath, image.ImageUrl);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                product.Images.Remove(image);
                await _context.SaveChangesAsync();
            }
        }
    }

    public async Task UpdateProductName(Guid id, string name)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            product.Name = name;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateProductCategory(Guid id, string category)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            product.Category = category;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateProductPrice(Guid id, decimal price)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            product.Price = price;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateProductStock(Guid id, int stock)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            product.Stock = stock;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateProductDescription(Guid id, string description)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            product.Description = description;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Images?> UploadImage(Guid productId, IFormFile file)
    {
        var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null || file == null || file.Length == 0)
        {
            return null;
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
        await _context.SaveChangesAsync();

        return image;
    }

    public async Task<bool> DeleteImage(Guid productId, Guid imageId)
    {
        var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null)
        {
            return false;
        }

        var image = product.Images.FirstOrDefault(i => i.Id == imageId);
        if (image == null)
        {
            return false;
        }

        var filePath = Path.Combine(_uploadPath, image.ImageUrl);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        product.Images.Remove(image);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<Images?> UpdateImage(Guid productId, Guid imageId, IFormFile file)
    {
        var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null || file == null || file.Length == 0)
        {
            return null;
        }

        var image = product.Images.FirstOrDefault(i => i.Id == imageId);
        if (image != null)
        {
            var filePath = Path.Combine(_uploadPath, image.ImageUrl);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            product.Images.Remove(image);
        }

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var newFilePath = Path.Combine(_uploadPath, fileName);

        using (var stream = new FileStream(newFilePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var newImage = new Images
        {
            Id = Guid.NewGuid(),
            ImageUrl = fileName,
            Alt = fileName
        };

        product.Images.Add(newImage);
        await _context.SaveChangesAsync();

        return newImage;
    }
}
