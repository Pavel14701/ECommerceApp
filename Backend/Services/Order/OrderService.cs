using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class OrderService : IOrderService
{
    private readonly IDbContextFactory _dbContextFactory;

    public OrderService(IDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<CreateOrderResultDto> CreateOrder(Order order)
    {
        using var context = _dbContextFactory.CreateDbContext();
        order.OrderDate = DateTime.UtcNow;

        foreach (var item in order.Items)
        {
            var product = await context.Products.FindAsync(item.ProductId);
            if (product == null || product.Stock < item.Quantity)
            {
                return new CreateOrderResultDto { Success = false, Message = "Product is not available or insufficient stock." };
            }
            product.Stock -= item.Quantity;
        }

        order.CalculateTotalAmount();
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        return new CreateOrderResultDto { Success = true, Message = "Order created successfully.", Order = order };
    }

    public async Task<ApplyDiscountResultDto> ApplyDiscount(Guid orderId, Discount discount)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var order = await context.Orders
            .Include(o => o.Items)
            .Include(o => o.Discounts)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return new ApplyDiscountResultDto { Success = false, Message = "Order not found.", OrderId = orderId };
        }

        order.Discounts.Add(discount);
        order.CalculateTotalAmount();
        await context.SaveChangesAsync();

        return new ApplyDiscountResultDto { Success = true, Message = "Discount applied successfully.", OrderId = orderId, Discount = discount };
    }

    public async Task<GetOrderByIdResultDto> GetOrderById(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var order = await context.Orders
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
