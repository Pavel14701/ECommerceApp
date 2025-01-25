using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

[Route("api/[controller]")]
[ApiController]
public class NewsUpdateController : ControllerBase
{
    private readonly IMessageSender _messageSender;

    public NewsUpdateController(IMessageSender messageSender)
    {
        _messageSender = messageSender;
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateNews([FromBody] News news)
    {
        var commandId = Guid.NewGuid();
        var updateNewsCommand = new UpdateNewsCommand
        {
            CommandId = commandId,
            News = news
        };

        var result = await _messageSender.SendCommandAndGetResponse<NewsUpdateResultDto>("news.exchange", "news.update", updateNewsCommand);
        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return BadRequest(result);
        }
    }

    [HttpPut("{newsId}/images/{imageId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateImage(Guid newsId, Guid imageId, [FromForm] IFormFile file)
    {
        var commandId = Guid.NewGuid();
        var updateImageCommand = new UpdateImageCommand
        {
            CommandId = commandId,
            NewsId = newsId,
            ImageId = imageId,
            File = file
        };

        var result = await _messageSender.SendCommandAndGetResponse<ImageUpdateResultDto>("news.exchange", "news.update.image", updateImageCommand);
        if (result.Message.Contains("successfully"))
        {
            return Ok(result);
        }
        else
        {
            return BadRequest(result);
        }
    }

    [HttpPut("{id}/title")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateTitle(Guid id, [FromBody] string title)
    {
        var commandId = Guid.NewGuid();
        var updateNewsTitleCommand = new UpdateNewsTitleCommand
        {
            CommandId = commandId,
            NewsId = id,
            Title = title
        };

        var result = await _messageSender.SendCommandAndGetResponse<NewsUpdateResultDto>("news.exchange", "news.update.title", updateNewsTitleCommand);
        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return BadRequest(result);
        }
    }

    [HttpPut("{id}/publishdate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdatePublishDate(Guid id, [FromBody] DateTime publishDate)
    {
        var commandId = Guid.NewGuid();
        var updateNewsPublishDateCommand = new UpdateNewsPublishDateCommand
        {
            CommandId = commandId,
            NewsId = id,
            PublishDate = publishDate
        };

        var result = await _messageSender.SendCommandAndGetResponse<NewsUpdateResultDto>("news.exchange", "news.update.publishdate", updateNewsPublishDateCommand);
        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return BadRequest(result);
        }
    }

    [HttpPut("{newsId}/content/{contentId}/text")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateContentText(Guid newsId, Guid contentId, [FromBody] string text)
    {
        var commandId = Guid.NewGuid();
        var updateNewsContentTextCommand = new UpdateNewsContentTextCommand
        {
            CommandId = commandId,
            NewsId = newsId,
            ContentId = contentId,
            Text = text
        };

        var result = await _messageSender.SendCommandAndGetResponse<NewsUpdateResultDto>("news.exchange", "news.update.contenttext", updateNewsContentTextCommand);
        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return BadRequest(result);
        }
    }
}
