using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[Route("api/[controller]")]
[ApiController]
public class NewsCreateController : ControllerBase
{
    private readonly IMessageSender _messageSender;

    public NewsCreateController(IMessageSender messageSender)
    {
        _messageSender = messageSender;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateNews([FromBody] News news)
    {
        var commandId = Guid.NewGuid();
        var createNewsCommand = new CreateNewsCommand
        {
            CommandId = commandId,
            News = news
        };

        var result = await _messageSender.SendCommandAndGetResponse<NewsCreationResultDto>("news.exchange", "news.create", createNewsCommand);
        return Ok(result);
    }

    [HttpPost("{id}/images")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddImageToNews(Guid id, [FromBody] Images image)
    {
        var commandId = Guid.NewGuid();
        var addImageToNewsCommand = new AddImageToNewsCommand
        {
            CommandId = commandId,
            NewsId = id,
            Image = image
        };

        var result = await _messageSender.SendCommandAndGetResponse<NewsCreationResultDto>("news.exchange", "news.addimage", addImageToNewsCommand);
        return Ok(result);
    }

    [HttpPost("{id}/uploadimage")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UploadImage(Guid id, [FromForm] IFormFile file)
    {
        var commandId = Guid.NewGuid();
        var uploadImageCommand = new UploadImageCommand
        {
            CommandId = commandId,
            Id = id,
            File = file
        };

        var result = await _messageSender.SendCommandAndGetResponse<ImageUploadResultDto>("news.exchange", "news.uploadimage", uploadImageCommand);
        return Ok(result);
    }
}