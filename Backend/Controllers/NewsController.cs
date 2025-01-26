using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[Route("api/[controller]")]
[ApiController]
public class newsController : ControllerBase
{
    private readonly IMessageSender _messageSender;

    public newsController(IMessageSender messageSender)
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