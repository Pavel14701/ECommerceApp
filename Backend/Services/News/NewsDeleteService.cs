using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class DeleteNewsService : IDeleteNewsService
{
    private readonly ApplicationDbContext _context;
    private readonly string _uploadPath;

    public DeleteNewsService(ApplicationDbContext context)
    {
        _context = context;
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<NewsDeletionResultDto> DeleteNews(Guid id)
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
            return new NewsDeletionResultDto { Success = true, Message = "News deleted successfully." };
        }
        return new NewsDeletionResultDto { Success = false, Message = "News not found." };
    }

    public async Task<ImageUpdateResultDto> RemoveImageFromNews(Guid newsId, Guid imageId)
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
                return new ImageUpdateResultDto { ImageId = imageId, ImageUrl = image.ImageUrl, Message = "Image removed successfully." };
            }
            return new ImageUpdateResultDto { Message = "Image not found." };
        }
        return new ImageUpdateResultDto { Message = "News not found." };
    }

    public async Task<ImageUpdateResultDto> DeleteImage(Guid newsId, Guid imageId)
    {
        var news = await _context.News.Include(n => n.Images).FirstOrDefaultAsync(n => n.Id == newsId);
        if (news == null)
        {
            return new ImageUpdateResultDto { Message = "News not found." };
        }

        var image = news.Images.FirstOrDefault(i => i.Id == imageId);
        if (image == null)
        {
            return new ImageUpdateResultDto { Message = "Image not found." };
        }

        var filePath = Path.Combine(_uploadPath, image.ImageUrl);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        news.Images.Remove(image);
        await _context.SaveChangesAsync();

        return new ImageUpdateResultDto { ImageId = imageId, ImageUrl = image.ImageUrl, Message = "Image deleted successfully." };
    }
}