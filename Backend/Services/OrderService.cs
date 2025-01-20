using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;

    public OrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Order>> GetAllOrders()
    {
        if (_context.Orders == null)
        {
            throw new InvalidOperationException("DbSet<Order> is null.");
        }
        return await _context.Orders.ToListAsync();
    }

    public async Task<Order?> GetOrderById(Guid id) // Изменено на Task<Order?>
    {
        if (_context.Orders == null)
        {
            throw new InvalidOperationException("DbSet<Order> is null.");
        }
        return await _context.Orders.FindAsync(id);
    }

    public async Task AddOrder(Order order)
    {
        if (_context.Orders == null)
        {
            throw new InvalidOperationException("DbSet<Order> is null.");
        }
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
    }
}
