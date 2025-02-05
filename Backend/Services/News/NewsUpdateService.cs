using Microsoft.EntityFrameworkCore;
using Npgsql;

public class UpdateNewsService : IUpdateNewsService
{
    private readonly SessionIterator _sessionIterator;
    private readonly string _uploadPath;

    public UpdateNewsService(SessionIterator sessionIterator)
    {
        _sessionIterator = sessionIterator;
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
        await _sessionIterator.ExecuteAsync(async context =>
        {
            var commandText = @"
                UPDATE News
                SET Title = @Title, Content = @Content, PublishDate = @PublishDate
                WHERE Id = @Id
            ";
            await context.Database.ExecuteSqlRawAsync(commandText,
                new NpgsqlParameter("@Id", news.Id),
                new NpgsqlParameter("@Title", news.Title),
                new NpgsqlParameter("@Content", news.Content),
                new NpgsqlParameter("@PublishDate", news.PublishDate));
        });
        return new NewsUpdateResultDto
        {
            Success = true,
            Message = "News updated successfully."
        };
    }

    public async Task<ImageUpdateResultDto> UpdateImage(Guid newsId, Guid imageId, IFormFile file)
    {
        var news = await _sessionIterator.QueryAsync(async context =>
        {
            var commandText = @"
                SELECT * FROM News 
                WHERE Id = @NewsId
            ";
            return await context.News
                .FromSqlRaw(commandText, new NpgsqlParameter("@NewsId", newsId))
                .Include(n => n.Images)
                .FirstOrDefaultAsync();
        });
        if (news == null || file == null || file.Length == 0)
        {
            return new ImageUpdateResultDto
            {
                Message = "Invalid news ID or file."
            };
        }
        var image = news.Images.FirstOrDefault(i => i.Id == imageId);
        if (image != null)
        {
            var filePath = Path.Combine(_uploadPath, image.ImageUrl);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            await _sessionIterator.ExecuteAsync(async context =>
            {
                var deleteImageCommand = @"
                    DELETE FROM Images 
                    WHERE Id = @ImageId
                ";
                await context.Database.ExecuteSqlRawAsync(
                    deleteImageCommand,
                    new NpgsqlParameter("@ImageId", imageId)
                );
            });
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
        await _sessionIterator.ExecuteAsync(async context =>
        {
            var insertImageCommand = @"
                INSERT INTO Images (Id, ImageUrl, Alt, NewsId)
                VALUES (@Id, @ImageUrl, @Alt, @NewsId)
            ";
            await context.Database.ExecuteSqlRawAsync(insertImageCommand,
                new NpgsqlParameter("@Id", newImage.Id),
                new NpgsqlParameter("@ImageUrl", newImage.ImageUrl),
                new NpgsqlParameter("@Alt", newImage.Alt),
                new NpgsqlParameter("@NewsId", newsId));
        });
        return new ImageUpdateResultDto
        {
            ImageId = newImage.Id,
            ImageUrl = newImage.ImageUrl,
            Message = "Image updated successfully."
        };
    }

    public async Task<NewsUpdateResultDto> UpdateNewsTitle(Guid id, string title)
    {
        var news = await _sessionIterator.QueryAsync(async context =>
        {
            var commandText = @"
                SELECT * FROM News 
                WHERE Id = @Id
            ";
            return await context.News
                .FromSqlRaw(commandText, new NpgsqlParameter("@Id", id))
                .FirstOrDefaultAsync();
        });
        if (news != null)
        {
            await _sessionIterator.ExecuteAsync(async context =>
            {
                var updateTitleCommand = @"
                    UPDATE News 
                    SET Title = @Title 
                    WHERE Id = @Id
                ";
                await context.Database.ExecuteSqlRawAsync(updateTitleCommand,
                    new NpgsqlParameter("@Title", title),
                    new NpgsqlParameter("@Id", id));
            });
            return new NewsUpdateResultDto
            {
                Success = true,
                Message = "News title updated successfully."
            };
        }
        return new NewsUpdateResultDto
        {
            Success = false,
            Message = "News not found."
        };
    }

    public async Task<NewsUpdateResultDto> UpdateNewsPublishDate(Guid id, DateTime publishDate)
    {
        var news = await _sessionIterator.QueryAsync(async context =>
        {
            var commandText = @"
                SELECT * FROM News 
                WHERE Id = @Id
            ";
            return await context.News
                .FromSqlRaw(commandText, new NpgsqlParameter("@Id", id))
                .FirstOrDefaultAsync();
        });
        if (news != null)
        {
            await _sessionIterator.ExecuteAsync(async context =>
            {
                var updateDateCommand = @"
                    UPDATE News 
                    SET PublishDate = @PublishDate
                    WHERE Id = @Id
                ";
                await context.Database.ExecuteSqlRawAsync(updateDateCommand,
                    new NpgsqlParameter("@PublishDate", publishDate),
                    new NpgsqlParameter("@Id", id));
            });
            return new NewsUpdateResultDto
            {
                Success = true,
                Message = "Publish date updated successfully."
            };
        }
        return new NewsUpdateResultDto
        {
            Success = false,
            Message = "News not found."
        };
    }

    public async Task<NewsUpdateResultDto> UpdateNewsContentText(Guid newsId, Guid contentId, string text)
    {
        var news = await _sessionIterator.QueryAsync(async context =>
        {
            var commandText = @"
                SELECT * FROM News 
                WHERE Id = @NewsId
            ";
            return await context.News
                .FromSqlRaw(commandText, new NpgsqlParameter("@NewsId", newsId))
                .Include(n => n.Content)
                .FirstOrDefaultAsync();
        });
        if (news != null)
        {
            var content = news.Content.FirstOrDefault(c => c.Id == contentId);
            if (content != null)
            {
                await _sessionIterator.ExecuteAsync(async context =>
                {
                    var updateContentCommand = @"
                        UPDATE Content 
                        SET Text = @Text 
                        WHERE Id = @ContentId
                    ";
                    await context.Database.ExecuteSqlRawAsync(updateContentCommand,
                        new NpgsqlParameter("@Text", text),
                        new NpgsqlParameter("@ContentId", contentId));
                });
                return new NewsUpdateResultDto
                {
                    Success = true,
                    Message = "Content text updated successfully."
                };
            }
            return new NewsUpdateResultDto
            {
                Success = false,
                Message = "Content not found."
            };
        }
        return new NewsUpdateResultDto
        {
            Success = false,
            Message = "News not found."
        };
    }
}
