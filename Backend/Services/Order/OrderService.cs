public class CreateOrderParamsDto
{
    public List<OrderItemInfo> Items { get; set; } = new List<OrderItemInfo>();
    public Guid UserId { get; set; }

    public string? DiscountCode { get; set; }
}


public class OrderResult : Result
{
    public OrderCreationResult? Order { get; set; }
}

public class ResultReadOrder : Result
{
    public OrderDto? Order { get; set; }
}



public interface IOrderService
{
    Task<OrderResult> CreateOrder(CreateOrderParamsDto paramsDto);
}


public class OrderService : IOrderService
{
    private readonly CreateCrud _createCrud;
    private readonly ReadCrud _readCrud;
    private readonly UpdateCrud _updateCrud;
    private readonly DeleteCrud _delCrud;

    public OrderService
    (
        CreateCrud createCrud,
        ReadCrud readCrud,
        UpdateCrud updateCrud,
        DeleteCrud delCrud
    )
    {
        _createCrud = createCrud;
        _readCrud = readCrud;
        _updateCrud = updateCrud;
        _delCrud = delCrud;
    }

    public async Task<OrderResult> CreateOrder(CreateOrderParamsDto paramsDto)
    { 
        try{
            var result = await _createCrud.CreateOrder(new CreateOrderParamsCrudDto
            {
                Id = Guid.NewGuid(),
                UserId = paramsDto.UserId,
                OrderDate = DateTime.UtcNow,
                Items  = paramsDto.Items,
                DiscountCode = paramsDto.DiscountCode
            });
            return new OrderResult
            {
                Order = result,
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new OrderResult
            {   
                Success = false,
                Message = $"Error: {ex}"
            };
        }
    }


    public async Task<ResultReadOrder> GetOrderById(Guid orderId)
    {
        try
        {   var result = await _readCrud.GetOrderById(orderId);
            return new ResultReadOrder{
                Success = true,
                Order = result?? throw new Exception("Nulled order")
            };
        }
        catch (Exception ex)
        {
            return new ResultReadOrder{
                Success = false, Message = $"Error: {ex}"
            };
        }
    }

    public async Task<Result> UmendOrder(Guid orderId)
    {
        try
        {
            await _delCrud.DeleteOrder(new DeleteOrderParamsCrudDto{
                Id = orderId
            }) ;
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
