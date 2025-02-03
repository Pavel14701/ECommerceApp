using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

public class CreateNewsService : ICreateNewsService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly string _uploadPath;

    public CreateNewsService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
        _uploadPath = Path.Combine(
            Directory.GetCurrentDirectory(), "wwwroot", "uploads"
        );
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<NewsCreationResultDto> AddNews(News news)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var commandText = @"
            INSERT INTO News (Id, Title, Content, PublishDate)
            VALUES (@Id, @Title, @Content, @PublishDate)";
        await context.Database.ExecuteSqlRawAsync(commandText,
            new SqlParameter("@Id", news.Id),
            new SqlParameter("@Title", news.Title),
            new SqlParameter("@Content", news.Content),
            new SqlParameter("@PublishDate", news.PublishDate));
        return new NewsCreationResultDto
        {
            NewsId = news.Id,
            Message = $"News with ID: {news.Id} has been created."
        };
    }

    public async Task<NewsCreationResultDto> AddImageToNews(Guid newsId, Images image)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var commandText = @"
            SELECT * FROM News 
            WHERE Id = @NewsId
        ";
        var news = await context.News
            .FromSqlRaw(
                commandText,
                new SqlParameter("@NewsId", newsId)
            )
            .Include(n => n.Images)
            .FirstOrDefaultAsync();
        if (news != null)
        {
            var insertImageCommand = @"
                INSERT INTO Images (Id, ImageUrl, Alt, NewsId)
                VALUES (@Id, @ImageUrl, @Alt, @NewsId)
            ";
            await context.Database.ExecuteSqlRawAsync(insertImageCommand,
                new SqlParameter("@Id", image.Id),
                new SqlParameter("@ImageUrl", image.ImageUrl),
                new SqlParameter("@Alt", image.Alt),
                new SqlParameter("@NewsId", newsId));
            return new NewsCreationResultDto
            {
                NewsId = newsId,
                Message = $@"
                    Image with ID: {image.Id} has been added
                    to News with ID: {newsId}.
                "
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
        using var context = _dbContextFactory.CreateDbContext();
        var commandText = @"
            SELECT * FROM News
            WHERE Id = @NewsId
        ";
        var news = await context.News
            .FromSqlRaw(commandText, new SqlParameter("@NewsId", newsId))
            .Include(n => n.Images)
            .FirstOrDefaultAsync();
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
        var insertImageCommand = @"
            INSERT INTO Images (Id, ImageUrl, Alt, NewsId)
            VALUES (@Id, @ImageUrl, @Alt, @NewsId)
        ";
        await context.Database.ExecuteSqlRawAsync(insertImageCommand,
            new SqlParameter("@Id", image.Id),
            new SqlParameter("@ImageUrl", image.ImageUrl),
            new SqlParameter("@Alt", image.Alt),
            new SqlParameter("@NewsId", newsId));
        return new ImageUploadResultDto
        {
            ImageId = image.Id,
            ImageUrl = fileName,
            Message = $@"
                Image with ID: {image.Id} has been uploaded and
                added to News with ID: {newsId}.
            "
        };
    }
}
