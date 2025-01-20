using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;

    public OrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CreateOrder(Order order)
    {
        order.OrderDate = DateTime.UtcNow;

        foreach (var item in order.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null || product.Stock < item.Quantity)
            {
                throw new InvalidOperationException("Product is not available or insufficient stock.");
            }
            product.Stock -= item.Quantity;
        }

        order.CalculateTotalAmount();
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
    }

    public async Task ApplyDiscount(Guid orderId, Discount discount)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Discounts)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
        }

        order.Discounts.Add(discount);
        order.CalculateTotalAmount();
        await _context.SaveChangesAsync();
    }

    public async Task<Order?> GetOrderById(Guid id) // Реализация метода
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Discounts)
            .FirstOrDefaultAsync(o => o.Id == id);
    }
}
