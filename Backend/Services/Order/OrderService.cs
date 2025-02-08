public class OrderCreationParamsDto
{

}


public class ResultOrderDto : Result
{
    public OrderDto? Order { get; protected set; }
    
    public ResultOrderDto(Result result, OrderDto? order)
    {
        Success = result.Success;
        Message = result.Message;
        Order = order;
    }
}



public interface IOrderService
{
    Task<Result> CreateOrder(OrderCreationParamsDto paramsDto);
}


public class OrderService : IOrderService
{
    private readonly SessionIterator _sessionIterator;
    private readonly CreateCrud _createCrud;
    private readonly ReadCrud _readCrud;
    private readonly UpdateCrud _updateCrud;
    private readonly DeleteCrud _delCrud;

    public OrderService
    (
        SessionIterator sessionIterator,
        CreateCrud createCrud,
        ReadCrud readCrud,
        UpdateCrud updateCrud,
        DeleteCrud delCrud
    )
    {
        _sessionIterator = sessionIterator;
        _createCrud = createCrud;
        _readCrud = readCrud;
        _updateCrud = updateCrud;
        _delCrud = delCrud;
    }

    public async Task<Result> CreateOrder(OrderCreationParamsDto paramsDto)
    {
        try{
            order.OrderDate = DateTime.UtcNow;
            order.CalculateTotalAmount();
            return new Result
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new Result
            {
                Success = false,
                Message = $"Error: {ex}"
            };
        }
    }



    public async Task<Result> ApplyDiscount(Guid orderId, string promocode)
    {
        try
        {
            return new Result
                {
                    Success = true
                };
        }
        catch (Exception ex)
        {
            return new Result
            {
                Success = false,
                Message = $"Error: {ex}"
            };
        }
    }


    public async Task<ResultOrderDto> GetOrderById(Guid orderId)
    {
        try
        {
            return new ResultOrderDto(
                new Result { Success = true}, null
            );
        }
        catch (Exception ex)
        {
            return new ResultOrderDto(
                new Result { Success = false, Message = $"Error: {ex}" }, null
            );
        }
    }

    public async Task<Result> UmendOrder(Guid orderId)
    {
        try
        {
            return new Result
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new Result
            {
                Success = false,
                Message = $"Error: {ex}"
            };
        }
    }
}
