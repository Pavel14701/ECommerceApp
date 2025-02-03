using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

public class OrderService : IOrderService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public OrderService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<CreateOrderResultDto> CreateOrder(Order order)
    {
        using var context = _dbContextFactory.CreateDbContext();
        order.OrderDate = DateTime.UtcNow;
        foreach (var item in order.Items)
        {
            var commandText = @"
                SELECT * FROM Products
                WHERE Id = @ProductId
            ";
            var product = await context.Products.FromSqlRaw(
                commandText, new SqlParameter(
                    "@ProductId", item.ProductId
                )
            ).SingleOrDefaultAsync();
            if (product == null || product.Stock < item.Quantity)
            {
                return new CreateOrderResultDto {
                    Success = false,
                    Message = "Product is not available or insufficient stock."
                };
            }
            var updateStockCommand = @"
                UPDATE Products 
                SET Stock = Stock - @Quantity 
                WHERE Id = @ProductId
            ";
            await context.Database.ExecuteSqlRawAsync(updateStockCommand,
                new SqlParameter("@Quantity", item.Quantity),
                new SqlParameter("@ProductId", item.ProductId));
        }
        order.CalculateTotalAmount();
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        return new CreateOrderResultDto {
            Success = true,
            Message = "Order created successfully.",
            Order = order
        };
    }

    public async Task<ApplyDiscountResultDto> ApplyDiscount(
        Guid orderId, Discount discount
    )
    {
        using var context = _dbContextFactory.CreateDbContext();
        var commandText = @"
            SELECT * FROM Orders 
            WHERE Id = @OrderId 
            INCLUDE Items 
            INCLUDE Discounts
        ";    
        var order = await context.Orders
            .FromSqlRaw(commandText, new SqlParameter("@OrderId", orderId))
            .Include(o => o.Items)
            .Include(o => o.Discounts)
            .FirstOrDefaultAsync();
        if (order == null)
        {
            return new ApplyDiscountResultDto {
                Success = false,
                Message = "Order not found.",
                OrderId = orderId
            };
        }
        order.Discounts.Add(discount);
        order.CalculateTotalAmount();
        await context.SaveChangesAsync();
        return new ApplyDiscountResultDto {
            Success = true,
            Message = "Discount applied successfully.",
            OrderId = orderId,
            Discount = discount
        };
    }

    public async Task<GetOrderByIdResultDto> GetOrderById(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var commandText = @"
            SELECT * FROM Orders 
            WHERE Id = @OrderId 
            INCLUDE Items 
            INCLUDE Discounts";
        var order = await context.Orders
            .FromSqlRaw(commandText, new SqlParameter("@OrderId", id))
            .Include(o => o.Items)
            .Include(o => o.Discounts)
            .FirstOrDefaultAsync();
        if (order == null)
        {
            return new GetOrderByIdResultDto {
                Success = false, Message = "Order not found."
            };
        }
        return new GetOrderByIdResultDto {
            Success = true,
            Message = "Order retrieved successfully.",
            Order = order
        };
    }
}
