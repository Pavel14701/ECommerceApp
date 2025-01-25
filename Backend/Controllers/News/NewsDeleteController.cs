using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class NewsDeleteController : ControllerBase
{
    private readonly IMessageSender _messageSender;

    public NewsDeleteController(IMessageSender messageSender)
    {
        _messageSender = messageSender;
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteNews(Guid id)
    {
        var commandId = Guid.NewGuid();
        var deleteNewsCommand = new DeleteNewsCommand
        {
            CommandId = commandId,
            NewsId = id
        };

        var result = await _messageSender.SendCommandAndGetResponse<NewsDeletionResultDto>("news.exchange", "news.delete", deleteNewsCommand);
        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return BadRequest(result);
        }
    }

    [HttpDelete("{newsId}/images/{imageId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteImage(Guid newsId, Guid imageId)
    {
        var commandId = Guid.NewGuid();
        var deleteImageCommand = new DeleteImageCommand
        {
            CommandId = commandId,
            ObjectId = newsId,
            ImageId = imageId
        };

        var result = await _messageSender.SendCommandAndGetResponse<ImageUpdateResultDto>("news.exchange", "news.delete.image", deleteImageCommand);
        if (result.Message.Contains("successfully"))
        {
            return Ok(result);
        }
        else
        {
            return BadRequest(result);
        }
    }
}
