using Microsoft.EntityFrameworkCore;

public class ReadNewsService : IReadNewsService
{
    private readonly ApplicationDbContext _context;

    public ReadNewsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedNewsDto> GetAllNews(int pageNumber, int pageSize)
    {
        var totalNewsCount = await _context.News.CountAsync();
        var news = await _context.News
                                .Include(n => n.Images)
                                .Include(n => n.Content)
                                .Skip((pageNumber - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();
        return new PagedNewsDto
        {
            News = news,
            TotalCount = totalNewsCount
        };
    }

    public async Task<NewsDto> GetNewsById(Guid id)
    {
        var news = await _context.News
                             .Include(n => n.Images)
                             .Include(n => n.Content)
                             .FirstOrDefaultAsync(n => n.Id == id);
        return new NewsDto
        {
            News = news
        };
    }
}
