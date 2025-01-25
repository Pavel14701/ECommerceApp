using System;
using System.Threading.Tasks;

public interface IOrderService
{
    Task<CreateOrderResultDto> CreateOrder(Order order);
    Task<ApplyDiscountResultDto> ApplyDiscount(Guid orderId, Discount discount);
    Task<GetOrderByIdResultDto> GetOrderById(Guid id);
}
