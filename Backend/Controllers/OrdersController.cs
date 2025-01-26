using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class orderController : ControllerBase
{
    private readonly IMessageSender _messageSender;

    public orderController(IMessageSender messageSender)
    {
        _messageSender = messageSender;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateOrder([FromBody] Order order)
    {
        var commandId = Guid.NewGuid();
        var createOrderCommand = new CreateOrderCommand
        {
            CommandId = commandId,
            Order = order
        };

        var result = await _messageSender.SendCommandAndGetResponse<CreateOrderResultDto>("orders.exchange", "orders.create", createOrderCommand);
        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return BadRequest(result);
        }
    }

    [HttpPost("{orderId}/discount")]
    [Authorize]
    public async Task<IActionResult> ApplyDiscount(Guid orderId, [FromBody] Discount discount)
    {
        var commandId = Guid.NewGuid();
        var applyDiscountCommand = new ApplyDiscountCommand
        {
            CommandId = commandId,
            OrderId = orderId,
            Discount = discount
        };

        var result = await _messageSender.SendCommandAndGetResponse<ApplyDiscountResultDto>("orders.exchange", "orders.applyDiscount", applyDiscountCommand);
        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return BadRequest(result);
        }
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var queryId = Guid.NewGuid();
        var getOrderByIdQuery = new GetOrderByIdQuery
        {
            QueryId = queryId,
            OrderId = id
        };

        var result = await _messageSender.SendCommandAndGetResponse<GetOrderByIdResultDto>("orders.exchange", "orders.getById", getOrderByIdQuery);
        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return NotFound(result);
        }
    }
}
