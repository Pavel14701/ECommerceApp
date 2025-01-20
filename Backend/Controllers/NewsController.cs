using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class NewsController : ControllerBase
{
    private readonly INewsService _newsService;

    public NewsController(INewsService newsService)
    {
        _newsService = newsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetNews([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var (news, totalNewsCount) = await _newsService.GetAllNews(pageNumber, pageSize);
        Response.Headers.Append("X-Total-Count", totalNewsCount.ToString());
        return Ok(news);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetNewsById(Guid id)
    {
        var news = await _newsService.GetNewsById(id);
        if (news == null)
        {
            return NotFound();
        }
        return Ok(news);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateNews([FromBody] News news)
    {
        await _newsService.AddNews(news);
        return CreatedAtAction(nameof(GetNewsById), new { id = news.Id }, news);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateNews(Guid id, [FromBody] News news)
    {
        if (id != news.Id)
        {
            return BadRequest();
        }

        await _newsService.UpdateNews(news);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteNews(Guid id)
    {
        await _newsService.DeleteNews(id);
        return NoContent();
    }

    [HttpPost("{id}/images")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddImageToNews(Guid id, [FromBody] Images image)
    {
        await _newsService.AddImageToNews(id, image);
        return NoContent();
    }

    [HttpDelete("{id}/images/{imageId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveImageFromNews(Guid id, Guid imageId)
    {
        await _newsService.RemoveImageFromNews(id, imageId);
        return NoContent();
    }

    [HttpPost("{id}/content")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddContentToNews(Guid id, [FromBody] Content content)
    {
        await _newsService.AddContentToNews(id, content);
        return NoContent();
    }

    [HttpDelete("{id}/content/{contentId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveContentFromNews(Guid id, Guid contentId)
    {
        await _newsService.RemoveContentFromNews(id, contentId);
        return NoContent();
    }

    [HttpPut("{id}/title")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateNewsTitle(Guid id, [FromBody] string title)
    {
        await _newsService.UpdateNewsTitle(id, title);
        return NoContent();
    }

    [HttpPut("{id}/publishDate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateNewsPublishDate(Guid id, [FromBody] DateTime publishDate)
    {
        await _newsService.UpdateNewsPublishDate(id, publishDate);
        return NoContent();
    }

    [HttpPut("{id}/content/{contentId}/text")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateNewsContentText(Guid id, Guid contentId, [FromBody] string text)
    {
        await _newsService.UpdateNewsContentText(id, contentId, text);
        return NoContent();
    }
}
