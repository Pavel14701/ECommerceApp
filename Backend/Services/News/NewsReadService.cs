using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

public class ReadNewsService : IReadNewsService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    public ReadNewsService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<PagedNewsDto> GetAllNews(int pageNumber, int pageSize)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var totalNewsCount = await context.News.CountAsync();
        var commandText = @"
            SELECT * FROM News
            ORDER BY PublishDate DESC
            OFFSET @Offset ROWS 
            FETCH NEXT @PageSize ROWS ONLY
        ";
        var news = await context.News
            .FromSqlRaw(commandText, 
                new SqlParameter("@Offset", (pageNumber - 1) * pageSize), 
                new SqlParameter("@PageSize", pageSize))
            .Include(n => n.Images)
            .Include(n => n.Content)
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
        var commandText = @"
            SELECT * FROM News 
            WHERE Id = @Id
        ";
        var news = await context.News
            .FromSqlRaw(
                commandText, 
                new SqlParameter("@Id", id)
            )
            .Include(n => n.Images)
            .Include(n => n.Content)
            .FirstOrDefaultAsync();
        return new NewsDto
        {
            News = news
        };
    }
}
