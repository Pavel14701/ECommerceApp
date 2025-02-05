using Npgsql;


public class CreateNewsCrudDto
{
    public required ApplicationDbContext Context { get; set; }
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public required DateTime PublishDatetime { get; set; }
    public required DateTime UpdateDatetime { get; set; }
}

public class CreateImageCrudDto
{
    public required ApplicationDbContext Context { get; set; } 
    public required Guid Id { get; set; }
    public required string ImageUrl { get; set; }
    public required string AltText { get; set; }
    public required Guid NewsId { get; set; }
}

public class CreateContentImageCrudDto
{
    public required ApplicationDbContext Context { get; set; }
    public required Guid Id { get; set; }
    public required Guid ImageId { get; set; }
    public required int BlockNumber { get; set; }
}

public class CreateContentTextCrudDto
{
    public required ApplicationDbContext Context { get; set; }
    public required Guid Id { get; set; }
    public required string TextContent { get; set; }
    public required int BlockNumber { get; set; }
    public Guid? NewsId { get; set; }
}



public interface ICreateCrud
{
    Task AddNews(CreateNewsCrudDto news);
    Task AddImage(CreateImageCrudDto image);
    Task AddNewsContentImage(CreateContentImageCrudDto image);
    Task AddNewsContentText(CreateContentTextCrudDto text);
}



public class CreateCrud : ICreateCrud
{
    private readonly SessionIterator _sessionIterator;
    public CreateCrud(SessionIterator sessionIterator)
    {
        _sessionIterator = sessionIterator;
    }

    public async Task AddNews(CreateNewsCrudDto news)
    {
        try
        {
            var commandTextNews = @"
                INSERT INTO news (
                    id, news_title, publish_datetime, update_datetime
                )
                VALUES (
                    @id, @news_title, @publish_datetime, @update_datetime
                )
            ";
            await _sessionIterator.ExecuteSqlRawAsync(news.Context, commandTextNews,
                new NpgsqlParameter("@id", news.Id),
                new NpgsqlParameter("@news_title", news.Title),
                new NpgsqlParameter("@publish_datetime", news.PublishDatetime),
                new NpgsqlParameter("@update_datetime", news.UpdateDatetime)
            );
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while adding news.", ex);
        }
    }

    public async Task AddImage(CreateImageCrudDto image)
    {
        try
        {
            var commandTextImage = @"
                INSERT INTO images (
                    id, image_url, alt
                )
                VALUES (
                    @id, @image_url, @alt
                );
                
                INSERT INTO news_images_relationship (
                    id, image_id, fk_news_id
                )
                VALUES (
                    @rel_id, @id, @fk_news_id
                );
            ";
            await _sessionIterator.ExecuteSqlRawAsync(image.Context, commandTextImage,
                new NpgsqlParameter("@id", image.Id),
                new NpgsqlParameter("@image_url", image.ImageUrl),
                new NpgsqlParameter("@alt", image.AltText),
                new NpgsqlParameter("@rel_id", Guid.NewGuid()),
                new NpgsqlParameter("@fk_news_id", image.NewsId)
            );
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while adding image.", ex);
        }
    }

    public async Task AddNewsContentImage(CreateContentImageCrudDto image)
    {
        try
        {
            var commandTextContentImage = @"
                INSERT INTO news_content (
                    id, text, fk_image_id, block_number
                )
                VALUES (
                    @id, @text, @fk_image_id, @block_number
                )
            ";
            await _sessionIterator.ExecuteSqlRawAsync(image.Context, commandTextContentImage,
                new NpgsqlParameter("@id", image.Id),
                new NpgsqlParameter("@text", DBNull.Value),
                new NpgsqlParameter("@fk_image_id", image.ImageId),
                new NpgsqlParameter("@block_number", image.BlockNumber)
            );
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while adding news content image.", ex);
        }
    }

    public async Task AddNewsContentText(CreateContentTextCrudDto text)
    {
        try
        {
            var checkCommandText = @"
                SELECT COUNT(*) FROM news_content 
                WHERE block_number = @block_number 
                AND text IS NOT NULL
            ";
            var existingContentCount = await _sessionIterator.ExecuteScalarAsync(text.Context, checkCommandText,
                new NpgsqlParameter("@block_number", text.BlockNumber));
            
            if ((long)existingContentCount > 0)
            {
                throw new InvalidOperationException($"Content with block number {text.BlockNumber} already exists.");
            }

            var commandTextContentText = @"
                INSERT INTO news_content (
                    id, text, fk_image_id, block_number
                )
                VALUES (
                    @id, @text, @fk_image_id, @block_number
                )
            ";
            await _sessionIterator.ExecuteSqlRawAsync(text.Context, commandTextContentText,
                new NpgsqlParameter("@id", text.Id),
                new NpgsqlParameter("@text", text.TextContent),
                new NpgsqlParameter("@fk_image_id", DBNull.Value),
                new NpgsqlParameter("@block_number", text.BlockNumber));
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while adding news content text.", ex);
        }
    }
}