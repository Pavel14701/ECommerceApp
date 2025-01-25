using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class CreateNewsService : ICreateNewsService
{
    private readonly ApplicationDbContext _context;
    private readonly string _uploadPath;

    public CreateNewsService(ApplicationDbContext context)
    {
        _context = context;
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<NewsCreationResultDto> AddNews(News news)
    {
        _context.News.Add(news);
        await _context.SaveChangesAsync();
        
        return new NewsCreationResultDto
        {
            NewsId = news.Id,
            Message = $"News with ID: {news.Id} has been created."
        };
    }

    public async Task<NewsCreationResultDto> AddImageToNews(Guid newsId, Images image)
    {
        var news = await _context.News.Include(n => n.Images).FirstOrDefaultAsync(n => n.Id == newsId);
        if (news != null)
        {
            news.Images.Add(image);
            await _context.SaveChangesAsync();
            
            return new NewsCreationResultDto
            {
                NewsId = newsId,
                Message = $"Image with ID: {image.Id} has been added to News with ID: {newsId}."
            };
        }

        return new NewsCreationResultDto
        {
            NewsId = newsId,
            Message = $"News with ID: {newsId} not found."
        };
    }

    public async Task<ImageUploadResultDto> UploadImage(Guid newsId, IFormFile file)
    {
        var news = await _context.News.Include(n => n.Images).FirstOrDefaultAsync(n => n.Id == newsId);
        if (news == null || file == null || file.Length == 0)
        {
            return new ImageUploadResultDto
            {
                Message = "Invalid news ID or file.",
                ImageUrl = string.Empty
            };
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
        
        return new ImageUploadResultDto
        {
            ImageId = image.Id,
            ImageUrl = fileName,
            Message = $"Image with ID: {image.Id} has been uploaded and added to News with ID: {newsId}."
        };
    }
}
