using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

public class UpdateNewsService : IUpdateNewsService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly string _uploadPath;

    public UpdateNewsService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
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

    public async Task<NewsUpdateResultDto> UpdateNews(News news)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var commandText = @"
            UPDATE News
            SET Title = @Title, Content = @Content, PublishDate = @PublishDate
            WHERE Id = @Id
        ";
        await context.Database.ExecuteSqlRawAsync(commandText,
            new SqlParameter("@Id", news.Id),
            new SqlParameter("@Title", news.Title),
            new SqlParameter("@Content", news.Content),
            new SqlParameter("@PublishDate", news.PublishDate));
        return new NewsUpdateResultDto {
            Success = true, Message = "News updated successfully."
        };
    }

    public async Task<ImageUpdateResultDto> UpdateImage(Guid newsId, Guid imageId, IFormFile file)
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
            var deleteImageCommand = @"
                DELETE FROM Images 
                WHERE Id = @ImageId
            ";
            await context.Database.ExecuteSqlRawAsync(
                deleteImageCommand,
                new SqlParameter("@ImageId", imageId)
            );
            news.Images.Remove(image);
        }
        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var newFilePath = Path.Combine(_uploadPath, fileName);
        using (var stream = new FileStream(newFilePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        var newImage = new Images {
            Id = Guid.NewGuid(),
            ImageUrl = fileName,
            Alt = fileName 
        };
        var insertImageCommand = @"
            INSERT INTO Images (Id, ImageUrl, Alt, NewsId)
            VALUES (@Id, @ImageUrl, @Alt, @NewsId)
        ";
        await context.Database.ExecuteSqlRawAsync(insertImageCommand,
            new SqlParameter("@Id", newImage.Id),
            new SqlParameter("@ImageUrl", newImage.ImageUrl),
            new SqlParameter("@Alt", newImage.Alt),
            new SqlParameter("@NewsId", newsId));
        return new ImageUpdateResultDto {
            ImageId = newImage.Id,
            ImageUrl = newImage.ImageUrl,
            Message = "Image updated successfully." 
        };
    }

    public async Task<NewsUpdateResultDto> UpdateNewsTitle(Guid id, string title)
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
            .FirstOrDefaultAsync();
        if (news != null)
        {
            var updateTitleCommand = @"
                UPDATE News 
                SET Title = @Title 
                WHERE Id = @Id
            ";
            await context.Database.ExecuteSqlRawAsync(updateTitleCommand,
                new SqlParameter("@Title", title),
                new SqlParameter("@Id", id));
            return new NewsUpdateResultDto {
                Success = true, 
                Message = "News title updated successfully."
            };
        }
        return new NewsUpdateResultDto {
            Success = false,
            Message = "News not found."
        };
    }

    public async Task<NewsUpdateResultDto> UpdateNewsPublishDate(Guid id, DateTime publishDate)
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
            .FirstOrDefaultAsync();
        if (news != null)
        {
            var updateDateCommand = @"
                UPDATE News 
                SET PublishDate = @PublishDate
                WHERE Id = @Id
            ";
            await context.Database.ExecuteSqlRawAsync(updateDateCommand,
                new SqlParameter("@PublishDate", publishDate),
                new SqlParameter("@Id", id));
            return new NewsUpdateResultDto {
                Success = true, 
                Message = "Publish date updated successfully."
            };
        }
        return new NewsUpdateResultDto {
            Success = false,
            Message = "News not found."
        };
    }

    public async Task<NewsUpdateResultDto> UpdateNewsContentText(Guid newsId, Guid contentId, string text)
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
            .Include(n => n.Content)
            .FirstOrDefaultAsync();
        if (news != null)
        {
            var content = news.Content.FirstOrDefault(
                c => c.Id == contentId
            );
            if (content != null)
            {
                var updateContentCommand = @"
                    UPDATE Content 
                    SET Text = @Text 
                    WHERE Id = @ContentId
                ";
                await context.Database.ExecuteSqlRawAsync(updateContentCommand,
                    new SqlParameter("@Text", text),
                    new SqlParameter("@ContentId", contentId));
                return new NewsUpdateResultDto {
                    Success = true, 
                    Message = "Content text updated successfully." 
                };
            }
            return new NewsUpdateResultDto {
                Success = false,
                Message = "Content not found." 
            };
        }
        return new NewsUpdateResultDto {
            Success = false, 
            Message = "News not found." 
        };
    }
}
