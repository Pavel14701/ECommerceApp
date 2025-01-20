using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
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

    [HttpPost("{id}/upload")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UploadImage(Guid id, [FromForm] IFormFile file)
    {
        var image = await _newsService.UploadImage(id, file);
        if (image == null)
        {
            return BadRequest("Failed to upload image.");
        }
        return Ok(image);
    }

    [HttpDelete("{id}/images/{imageId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteImage(Guid id, Guid imageId)
    {
        var result = await _newsService.DeleteImage(id, imageId);
        if (!result)
        {
            return BadRequest("Failed to delete image.");
        }
        return NoContent();
    }

    [HttpPut("{id}/images/{imageId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateImage(Guid id, Guid imageId, [FromForm] IFormFile file)
    {
        var image = await _newsService.UpdateImage(id, imageId, file);
        if (image == null)
        {
            return BadRequest("Failed to update image.");
        }
        return Ok(image);
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
