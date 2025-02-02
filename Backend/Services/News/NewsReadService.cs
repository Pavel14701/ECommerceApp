using Microsoft.EntityFrameworkCore;

public class ReadNewsService : IReadNewsService
{
        private readonly IDbContextFactory _dbContextFactory;

    public ReadNewsService(IDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<PagedNewsDto> GetAllNews(int pageNumber, int pageSize)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var totalNewsCount = await context.News.CountAsync();
        var news = await context.News
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
        using var context = _dbContextFactory.CreateDbContext();
        var news = await context.News
                             .Include(n => n.Images)
                             .Include(n => n.Content)
                             .FirstOrDefaultAsync(n => n.Id == id);
        return new NewsDto
        {
            News = news
        };
    }
}
