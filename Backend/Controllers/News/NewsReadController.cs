using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class NewsReadController : ControllerBase
{
    private readonly IMessageSender _messageSender;

    public NewsReadController(IMessageSender messageSender)
    {
        _messageSender = messageSender;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllNews([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var queryId = Guid.NewGuid();
        var getAllNewsQuery = new GetAllNewsQuery
        {
            QueryId = queryId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _messageSender.SendCommandAndGetResponse<PagedNewsDto>("news.exchange", "news.getall", getAllNewsQuery);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetNewsById(Guid id)
    {
        var queryId = Guid.NewGuid();
        var getNewsByIdQuery = new GetNewsByIdQuery
        {
            QueryId = queryId,
            NewsId = id
        };

        var result = await _messageSender.SendCommandAndGetResponse<News>("news.exchange", "news.getbyid", getNewsByIdQuery);
        return result != null ? Ok(result) : NotFound(new { Message = "News not found", NewsId = id });
    }
}
