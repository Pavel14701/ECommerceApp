using System.Collections.Generic;
using System.Threading.Tasks;

public interface IOrderService
{
    Task<IEnumerable<Order>> GetAllOrders();
    Task<Order?> GetOrderById(Guid id); // Изменено на Task<Order?>
    Task AddOrder(Order order);
}
