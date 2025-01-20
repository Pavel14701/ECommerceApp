using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllProducts()
    {
        if (_context.Products == null)
        {
            throw new InvalidOperationException("DbSet<Product> is null.");
        }
        return await _context.Products.ToListAsync();
    }

    public async Task<Product?> GetProductById(Guid id)
    {
        if (_context.Products == null)
        {
            throw new InvalidOperationException("DbSet<Product> is null.");
        }
        return await _context.Products.FindAsync(id);
    }

    public async Task AddProduct(Product product)
    {
        if (_context.Products == null)
        {
            throw new InvalidOperationException("DbSet<Product> is null.");
        }
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
    }
}
