using System;
using System.Threading.Tasks;

public interface IOrderService
{
    Task CreateOrder(Order order);
    Task ApplyDiscount(Guid orderId, Discount discount);
    Task<Order?> GetOrderById(Guid id);
}
