using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class NewsService : INewsService
{
    private readonly ApplicationDbContext _context;

    public NewsService(ApplicationDbContext context)
    {
        _context = context;
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
        var news = await _context.News.FindAsync(id);
        if (news != null)
        {
            _context.News.Remove(news);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddImageToNews(Guid newsId, Images image)
    {
        var news = await _context.News
                                 .Include(n => n.Images)
                                 .FirstOrDefaultAsync(n => n.Id == newsId);
        if (news != null)
        {
            news.Images.Add(image);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveImageFromNews(Guid newsId, Guid imageId)
    {
        var news = await _context.News
                                 .Include(n => n.Images)
                                 .FirstOrDefaultAsync(n => n.Id == newsId);
        if (news != null)
        {
            var image = news.Images.FirstOrDefault(i => i.Id == imageId);
            if (image != null)
            {
                news.Images.Remove(image);
                await _context.SaveChangesAsync();
            }
        }
    }

    public async Task AddContentToNews(Guid newsId, Content content)
    {
        var news = await _context.News
                                 .Include(n => n.Content)
                                 .FirstOrDefaultAsync(n => n.Id == newsId);
        if (news != null)
        {
            news.Content.Add(content);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveContentFromNews(Guid newsId, Guid contentId)
    {
        var news = await _context.News
                                 .Include(n => n.Content)
                                 .FirstOrDefaultAsync(n => n.Id == newsId);
        if (news != null)
        {
            var content = news.Content.FirstOrDefault(c => c.Id == contentId);
            if (content != null)
            {
                news.Content.Remove(content);
                await _context.SaveChangesAsync();
            }
        }
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
