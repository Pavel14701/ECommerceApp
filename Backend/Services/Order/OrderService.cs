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

    public async Task<CreateOrderResultDto> CreateOrder(Order order)
    {
        order.OrderDate = DateTime.UtcNow;

        foreach (var item in order.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null || product.Stock < item.Quantity)
            {
                return new CreateOrderResultDto { Success = false, Message = "Product is not available or insufficient stock." };
            }
            product.Stock -= item.Quantity;
        }

        order.CalculateTotalAmount();
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return new CreateOrderResultDto { Success = true, Message = "Order created successfully.", Order = order };
    }

    public async Task<ApplyDiscountResultDto> ApplyDiscount(Guid orderId, Discount discount)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Discounts)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return new ApplyDiscountResultDto { Success = false, Message = "Order not found.", OrderId = orderId };
        }

        order.Discounts.Add(discount);
        order.CalculateTotalAmount();
        await _context.SaveChangesAsync();

        return new ApplyDiscountResultDto { Success = true, Message = "Discount applied successfully.", OrderId = orderId, Discount = discount };
    }

    public async Task<GetOrderByIdResultDto> GetOrderById(Guid id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Discounts)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return new GetOrderByIdResultDto { Success = false, Message = "Order not found." };
        }

        return new GetOrderByIdResultDto { Success = true, Message = "Order retrieved successfully.", Order = order };
    }
}
