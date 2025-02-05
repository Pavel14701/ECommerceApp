using Microsoft.EntityFrameworkCore;
using Npgsql;
public class OrderService : IOrderService
{
    private readonly SessionIterator _sessionIterator;

    public OrderService(SessionIterator sessionIterator)
    {
        _sessionIterator = sessionIterator;
    }

    public async Task<CreateOrderResultDto> CreateOrder(Order order)
    {
        var result = new CreateOrderResultDto
        {
            Success = false,
            Message = "Failed to create order."
        };
        await _sessionIterator.ExecuteAsync(async context =>
        {
            order.OrderDate = DateTime.UtcNow;
            foreach (var item in order.Items)
            {
                var commandText = @"
                    SELECT * FROM Products
                    WHERE Id = @ProductId
                ";
                var product = await context.Products.FromSqlRaw(commandText,
                    new NpgsqlParameter("@ProductId", item.ProductId)
                ).SingleOrDefaultAsync();
                if (product == null || product.Stock < item.Quantity)
                {
                    result = new CreateOrderResultDto
                    {
                        Success = false,
                        Message = "Product is not available or insufficient stock."
                    };
                    return;
                }
                var updateStockCommand = @"
                    UPDATE Products 
                    SET Stock = Stock - @Quantity 
                    WHERE Id = @ProductId
                ";
                await context.Database.ExecuteSqlRawAsync(updateStockCommand,
                    new NpgsqlParameter("@Quantity", item.Quantity),
                    new NpgsqlParameter("@ProductId", item.ProductId));
            }
            order.CalculateTotalAmount();
            context.Orders.Add(order);
            await context.SaveChangesAsync();
            result = new CreateOrderResultDto
            {
                Success = true,
                Message = "Order created successfully.",
                Order = order
            };
        });
        return result;
    }


    public async Task<ApplyDiscountResultDto> ApplyDiscount(
        Guid orderId, Discount discount
    )
    {
        var commandText = @"
            SELECT * FROM Orders 
            WHERE Id = @OrderId 
            INCLUDE Items 
            INCLUDE Discounts
        ";
        var order = await _sessionIterator.QueryAsync(async context =>
        {
            return await context.Orders
                .FromSqlRaw(commandText, new NpgsqlParameter("@OrderId", orderId))
                .Include(o => o.Items)
                .Include(o => o.Discounts)
                .FirstOrDefaultAsync();
        });
        if (order == null)
        {
            return new ApplyDiscountResultDto
            {
                Success = false,
                Message = "Order not found.",
                OrderId = orderId
            };
        }
        await _sessionIterator.ExecuteAsync(async context =>
        {
            order.Discounts.Add(discount);
            order.CalculateTotalAmount();
            await context.SaveChangesAsync();
        });
        return new ApplyDiscountResultDto
        {
            Success = true,
            Message = "Discount applied successfully.",
            OrderId = orderId,
            Discount = discount
        };
    }

    public async Task<GetOrderByIdResultDto> GetOrderById(Guid id)
    {
        var commandText = @"
            SELECT * FROM Orders 
            WHERE Id = @OrderId 
            INCLUDE Items 
            INCLUDE Discounts";
        var order = await _sessionIterator.QueryAsync(async context =>
        {
            return await context.Orders
                .FromSqlRaw(commandText, new NpgsqlParameter("@OrderId", id))
                .Include(o => o.Items)
                .Include(o => o.Discounts)
                .FirstOrDefaultAsync();
        });
        if (order == null)
        {
            return new GetOrderByIdResultDto
            {
                Success = false,
                Message = "Order not found."
            };
        }
        return new GetOrderByIdResultDto
        {
            Success = true,
            Message = "Order retrieved successfully.",
            Order = order
        };
    }
}
