using Microsoft.EntityFrameworkCore;
using Npgsql;

public class ReadNewsService : IReadNewsService
{
    private readonly SessionIterator _sessionIterator;

    public ReadNewsService(SessionIterator sessionIterator)
    {
        _sessionIterator = sessionIterator;
    }

    public async Task<PagedNewsDto> GetAllNews(int pageNumber, int pageSize)
    {
        var countCommandText = "SELECT COUNT(*) FROM News";
        var totalNewsCount = await _sessionIterator.ExecuteScalarAsync(countCommandText);
        var commandText = @"
            SELECT * FROM News
            ORDER BY PublishDate DESC
            OFFSET @Offset ROWS 
            FETCH NEXT @PageSize ROWS ONLY
        ";
        var news = await _sessionIterator.QueryAsync(async context =>
        {
            return await context.News
                .FromSqlRaw(commandText, 
                    new NpgsqlParameter("@Offset", (pageNumber - 1) * pageSize), 
                    new NpgsqlParameter("@PageSize", pageSize))
                .Include(n => n.Images)
                .Include(n => n.Content)
                .ToListAsync();
        });
        return new PagedNewsDto
        {
            News = news,
            TotalCount = totalNewsCount
        };
    }

    public async Task<NewsDto> GetNewsById(Guid id)
    {
        var commandText = @"
            SELECT * FROM News 
            WHERE Id = @Id
        ";
        var news = await _sessionIterator.QueryAsync(async context =>
        {
            return await context.News
                .FromSqlRaw(commandText, new NpgsqlParameter("@Id", id))
                .Include(n => n.Images)
                .Include(n => n.Content)
                .FirstOrDefaultAsync();
        });
        return new NewsDto
        {
            News = news
        };
    }
}
