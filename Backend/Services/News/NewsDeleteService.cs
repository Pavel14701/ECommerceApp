using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

public class DeleteNewsService : IDeleteNewsService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly string _uploadPath;

    public DeleteNewsService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
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

    public async Task<NewsDeletionResultDto> DeleteNews(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var commandText = @"
            SELECT * FROM News
            WHERE Id = @NewsId
        ";
        var news = await context.News
            .FromSqlRaw(
                commandText,
                new SqlParameter("@NewsId", id)
            )
            .Include(n => n.Images)
            .FirstOrDefaultAsync();
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
            var deleteNewsCommand = @"
                DELETE FROM News 
                WHERE Id = @NewsId
            ";
            await context.Database.ExecuteSqlRawAsync(
                deleteNewsCommand, new SqlParameter("@NewsId", id)
            );
            return new NewsDeletionResultDto {
                Success = true,
                Message = "News deleted successfully."
            };
        }
        return new NewsDeletionResultDto {
            Success = false,
            Message = "News not found."
        };
    }

    public async Task<ImageUpdateResultDto> RemoveImageFromNews(Guid newsId, Guid imageId)
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
                return new ImageUpdateResultDto {
                    ImageId = imageId,
                    ImageUrl = image.ImageUrl,
                    Message = "Image removed successfully."
                };
            }
            return new ImageUpdateResultDto {
                Message = "Image not found."
            };
        }
        return new ImageUpdateResultDto {
            Message = "News not found."
        };
    }

    public async Task<ImageUpdateResultDto> DeleteImage(Guid newsId, Guid imageId)
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
        var deleteImageCommand = @"
            DELETE FROM Images 
            WHERE Id = @ImageId
        ";
        await context.Database.ExecuteSqlRawAsync(
            deleteImageCommand, 
            new SqlParameter("@ImageId", imageId)
        );
        return new ImageUpdateResultDto {
            ImageId = imageId,
            ImageUrl = image.ImageUrl,
            Message = "Image deleted successfully."
        };
    }
}
