using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class NewsService : INewsService
{
    private readonly ApplicationDbContext _context;
    private readonly string _uploadPath;

    public NewsService(ApplicationDbContext context)
    {
        _context = context;
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<(IEnumerable<News>, int)> GetAllNews(int pageNumber, int pageSize)
    {
        var totalNewsCount = await _context.News.CountAsync();
        var news = await _context.News
                                .Include(n => n.Images)
                                .Include(n => n.Content)
                                .Skip((pageNumber - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();
        return (news, totalNewsCount);
    }

    public async Task<News?> GetNewsById(Guid id)
    {
        return await _context.News
                             .Include(n => n.Images)
                             .Include(n => n.Content)
                             .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task AddNews(News news)
    {
        _context.News.Add(news);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateNews(News news)
    {
        _context.News.Update(news);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteNews(Guid id)
    {
        var news = await _context.News.Include(n => n.Images).FirstOrDefaultAsync(n => n.Id == id);
        if (news != null)
        {
            foreach (var image in news.Images)
            {
                var filePath = Path.Combine(_uploadPath, image.ImageUrl);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            _context.News.Remove(news);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddImageToNews(Guid newsId, Images image)
    {
        var news = await _context.News.Include(n => n.Images).FirstOrDefaultAsync(n => n.Id == newsId);
        if (news != null)
        {
            news.Images.Add(image);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveImageFromNews(Guid newsId, Guid imageId)
    {
        var news = await _context.News.Include(n => n.Images).FirstOrDefaultAsync(n => n.Id == newsId);
        if (news != null)
        {
            var image = news.Images.FirstOrDefault(i => i.Id == imageId);
            if (image != null)
            {
                var filePath = Path.Combine(_uploadPath, image.ImageUrl);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                news.Images.Remove(image);
                await _context.SaveChangesAsync();
            }
        }
    }

    public async Task<Images?> UploadImage(Guid newsId, IFormFile file)
    {
        var news = await _context.News.Include(n => n.Images).FirstOrDefaultAsync(n => n.Id == newsId);
        if (news == null || file == null || file.Length == 0)
        {
            return null;
        }

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(_uploadPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var image = new Images
        {
            Id = Guid.NewGuid(),
            ImageUrl = fileName,
            Alt = fileName
        };

        news.Images.Add(image);
        await _context.SaveChangesAsync();

        return image;
    }

    public async Task<bool> DeleteImage(Guid newsId, Guid imageId)
    {
        var news = await _context.News.Include(n => n.Images).FirstOrDefaultAsync(n => n.Id == newsId);
        if (news == null)
        {
            return false;
        }

        var image = news.Images.FirstOrDefault(i => i.Id == imageId);
        if (image == null)
        {
            return false;
        }

        var filePath = Path.Combine(_uploadPath, image.ImageUrl);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        news.Images.Remove(image);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<Images?> UpdateImage(Guid newsId, Guid imageId, IFormFile file)
    {
        var news = await _context.News.Include(n => n.Images).FirstOrDefaultAsync(n => n.Id == newsId);
        if (news == null || file == null || file.Length == 0)
        {
            return null;
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

        var newImage = new Images
        {
            Id = Guid.NewGuid(),
            ImageUrl = fileName,
            Alt = fileName
        };

        news.Images.Add(newImage);
        await _context.SaveChangesAsync();

        return newImage;
    }

    public async Task UpdateNewsTitle(Guid id, string title)
    {
        var news = await _context.News.FindAsync(id);
        if (news != null)
        {
            news.Title = title;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateNewsPublishDate(Guid id, DateTime publishDate)
    {
        var news = await _context.News.FindAsync(id);
        if (news != null)
        {
            news.PublishDate = publishDate;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateNewsContentText(Guid newsId, Guid contentId, string text)
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
            }
        }
    }
}
