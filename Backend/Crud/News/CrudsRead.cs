using Microsoft.EntityFrameworkCore;
using Npgsql;


public class CheckNewsCrudDto
{
    public required Guid NewsId { get; set; }
}
public class ReadNewsIdDto
{
    public required string Title { get; set; }
    public required DateTime PublishDatetime { get; set; }
}

public class NewsReadResultDto
{
    public required Guid Id { get; set; }

}



public interface IReadCrud
{
    Task<Result> CheckNews(CheckNewsCrudDto news);
}




public class ReadCrud : IReadCrud
{
    private readonly SessionIterator _sessionIterator;
    public ReadCrud(SessionIterator sessionIterator)
    {
        _sessionIterator = sessionIterator;
    }

    public async Task<Result> CheckNews(CheckNewsCrudDto news)
    {
        var commandText = @"
            SELECT n.id, n.news_title, n.publish_datetime, n.update_datetime, ni.id AS ImageId, ni.image_url, ni.alt 
            FROM news n
            LEFT JOIN news_images_relationship nir ON n.id = nir.fk_news_id
            LEFT JOIN images ni ON nir.image_id = ni.id
            WHERE n.id = @NewsId
        ";
        
        var result = await _sessionIterator.ReadAsync(async context =>
        {
            var connection = context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.Parameters.Add(new NpgsqlParameter("@NewsId", news.NewsId));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new 
                {
                    Id = reader.GetGuid(0),
                    NewsTitle = reader.GetString(1),
                    PublishDatetime = reader.GetDateTime(2),
                    UpdateDatetime = reader.GetDateTime(3),
                    ImageId = reader.IsDBNull(4) ? null : (Guid?)reader.GetGuid(4),
                    ImageUrl = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Alt = reader.IsDBNull(6) ? null : reader.GetString(6)
                };
            }
            return null;
        });

        if (result != null)
        {
            return new Result
            {
                Success = true,
            };
        }
        return new Result
        {
            Success = false,
        };
    }


    public async Task<NewsReadResultDto?> GetNewsIdByTitleAndDate(ReadNewsIdDto paramsDto)
    {
        var commandText = @"
            SELECT n.id, n.news_title, n.publish_datetime 
            FROM news n
            WHERE n.news_title = @Title AND n.publish_datetime = @PublishDatetime
        ";
        
        var result = await _sessionIterator.ReadAsync(async context =>
        {
            var connection = context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.Parameters.Add(new NpgsqlParameter("@Title", paramsDto.Title));
            command.Parameters.Add(new NpgsqlParameter("@PublishDatetime", paramsDto.PublishDatetime));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new NewsReadResultDto
                {
                    Id = reader.GetGuid(0)
                };
            }
            return null;
        });
        
        return result;
    }
}
