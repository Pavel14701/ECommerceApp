using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class UpdateNewsService : IUpdateNewsService
{
    private readonly IDbContextFactory _dbContextFactory;
    private readonly string _uploadPath;

    public UpdateNewsService(IDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<NewsUpdateResultDto> UpdateNews(News news)
    {
        using var context = _dbContextFactory.CreateDbContext();
        context.News.Update(news);
        await context.SaveChangesAsync();
        return new NewsUpdateResultDto { Success = true, Message = "News updated successfully." };
    }

    public async Task<ImageUpdateResultDto> UpdateImage(Guid newsId, Guid imageId, IFormFile file)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var news = await context.News.Include(n => n.Images).FirstOrDefaultAsync(n => n.Id == newsId);
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
        await context.SaveChangesAsync();
        return new ImageUpdateResultDto { ImageId = newImage.Id, ImageUrl = newImage.ImageUrl, Message = "Image updated successfully." };
    }

    public async Task<NewsUpdateResultDto> UpdateNewsTitle(Guid id, string title)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var news = await context.News.FindAsync(id);
        if (news != null)
        {
            news.Title = title;
            await context.SaveChangesAsync();
            return new NewsUpdateResultDto { Success = true, Message = "News title updated successfully." };
        }
        return new NewsUpdateResultDto { Success = false, Message = "News not found." };
    }

    public async Task<NewsUpdateResultDto> UpdateNewsPublishDate(Guid id, DateTime publishDate)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var news = await context.News.FindAsync(id);
        if (news != null)
        {
            news.PublishDate = publishDate;
            await context.SaveChangesAsync();
            return new NewsUpdateResultDto { Success = true, Message = "Publish date updated successfully." };
        }
        return new NewsUpdateResultDto { Success = false, Message = "News not found." };
    }

    public async Task<NewsUpdateResultDto> UpdateNewsContentText(Guid newsId, Guid contentId, string text)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var news = await context.News
            .Include(n => n.Content)
            .FirstOrDefaultAsync(n => n.Id == newsId);
        if (news != null)
        {
            var content = news.Content.FirstOrDefault(c => c.Id == contentId);
            if (content != null)
            {
                content.Text = text;
                await context.SaveChangesAsync();
                return new NewsUpdateResultDto { Success = true, Message = "Content text updated successfully." };
            }
            return new NewsUpdateResultDto { Success = false, Message = "Content not found." };
        }
        return new NewsUpdateResultDto { Success = false, Message = "News not found." };
    }
}
