using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class UpdateNewsService : IUpdateNewsService
{
    private readonly ApplicationDbContext _context;
    private readonly string _uploadPath;

    public UpdateNewsService(ApplicationDbContext context)
    {
        _context = context;
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<NewsUpdateResultDto> UpdateNews(News news)
    {
        _context.News.Update(news);
        await _context.SaveChangesAsync();
        return new NewsUpdateResultDto { Success = true, Message = "News updated successfully." };
    }

    public async Task<ImageUpdateResultDto> UpdateImage(Guid newsId, Guid imageId, IFormFile file)
    {
        var news = await _context.News.Include(n => n.Images).FirstOrDefaultAsync(n => n.Id == newsId);
        if (news == null || file == null || file.Length == 0)
        {
            return new ImageUpdateResultDto { Message = "Invalid news ID or file." };
        }

        var image = news.Images.FirstOrDefault(i => i.Id == imageId);
        if (image != null)
        {
            var filePath = Path.Combine(_uploadPath, image.ImageUrl);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            news.Images.Remove(image);
        }

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var newFilePath = Path.Combine(_uploadPath, fileName);
        using (var stream = new FileStream(newFilePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var newImage = new Images { Id = Guid.NewGuid(), ImageUrl = fileName, Alt = fileName };
        news.Images.Add(newImage);
        await _context.SaveChangesAsync();
        return new ImageUpdateResultDto { ImageId = newImage.Id, ImageUrl = newImage.ImageUrl, Message = "Image updated successfully." };
    }

    public async Task<NewsUpdateResultDto> UpdateNewsTitle(Guid id, string title)
    {
        var news = await _context.News.FindAsync(id);
        if (news != null)
        {
            news.Title = title;
            await _context.SaveChangesAsync();
            return new NewsUpdateResultDto { Success = true, Message = "News title updated successfully." };
        }
        return new NewsUpdateResultDto { Success = false, Message = "News not found." };
    }

    public async Task<NewsUpdateResultDto> UpdateNewsPublishDate(Guid id, DateTime publishDate)
    {
        var news = await _context.News.FindAsync(id);
        if (news != null)
        {
            news.PublishDate = publishDate;
            await _context.SaveChangesAsync();
            return new NewsUpdateResultDto { Success = true, Message = "Publish date updated successfully." };
        }
        return new NewsUpdateResultDto { Success = false, Message = "News not found." };
    }

    public async Task<NewsUpdateResultDto> UpdateNewsContentText(Guid newsId, Guid contentId, string text)
    {
        var news = await _context.News
            .Include(n => n.Content)
            .FirstOrDefaultAsync(n => n.Id == newsId);
        if (news != null)
        {
            var content = news.Content.FirstOrDefault(c => c.Id == contentId);
            if (content != null)
            {
                content.Text = text;
                await _context.SaveChangesAsync();
                return new NewsUpdateResultDto { Success = true, Message = "Content text updated successfully." };
            }
            return new NewsUpdateResultDto { Success = false, Message = "Content not found." };
        }
        return new NewsUpdateResultDto { Success = false, Message = "News not found." };
    }
}
