using Microsoft.EntityFrameworkCore;
using Npgsql;

public class DeleteNewsParamsCrudDto : NewsReadResultDto
{
    public required ApplicationDbContext Context { get; set; }
}

public class DeleteImageParamsCrudDto : DeleteNewsParamsCrudDto
{}

public class DeleteTextParamsCrudDto : DeleteNewsParamsCrudDto
{}

public class DeleteTextNewsParamsDto : DeleteNewsParamsCrudDto
{}

public class DeleteNewsImageParamsDto : DeleteNewsParamsCrudDto
{
    public required int BlockNumber { get; set;}
}

public class DeleteTextBlockImageParamsDto : DeleteNewsImageParamsDto
{}






public interface IDeleteCrud
{
    Task DeleteNews(DeleteNewsParamsCrudDto paramsDto);
    Task<List<Guid>> DeleteAllImagesByNewsId(DeleteImageParamsCrudDto paramsDto);
    Task DeleteAllTextBlocksByNewsId(DeleteTextParamsCrudDto paramsDto);
    Task<List<Guid>> DeleteImagesByNewsIdAndBlockNumber(DeleteNewsImageParamsDto paramsDto);
    Task DeleteTextBlockByNewsIdAndBlockNumber(DeleteTextBlockImageParamsDto paramsDto);
    Task DeleteOrder(ApplicationDbContext context, Guid orderId);
    Task<List<Guid>> DeleteProduct(ApplicationDbContext context, Guid productId);
    Task DeleteProductImages(ApplicationDbContext context, Guid productId, List<Guid> imageIds);

}





public class DeleteCrud : IDeleteCrud
{
    private readonly SessionIterator _sessionIterator;
    public DeleteCrud(SessionIterator sessionIterator)
    {
        _sessionIterator = sessionIterator;
    }



    public async Task DeleteNews(DeleteNewsParamsCrudDto paramsDto)
    {
        try
        {
            var commandText = @"
                WITH ContentIds AS (
                    SELECT fk_content
                    FROM news_relationships
                    WHERE fk_news = @NewsId
                ),
                ImageIds AS (
                    SELECT image_id
                    FROM news_images_relationship
                    WHERE fk_news_id = @NewsId
                )
                DELETE FROM news_relationships
                WHERE fk_news = @NewsId;

                DELETE FROM news_images_relationship
                WHERE fk_news_id = @NewsId;

                DELETE FROM news_content
                WHERE id IN (SELECT fk_content FROM ContentIds);

                DELETE FROM images
                WHERE id IN (SELECT image_id FROM ImageIds);

                DELETE FROM news
                WHERE id = @NewsId;
            ";
            await _sessionIterator.ExecuteSqlRawAsync(paramsDto.Context, commandText,
                new NpgsqlParameter("@NewsId", paramsDto.Id));
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while deleting the news.", ex);
        }
    }

    public async Task<List<Guid>> DeleteAllImagesByNewsId(DeleteImageParamsCrudDto paramsDto)
    {
        try
        {
            var commandText = @"
                WITH ImageIds AS (
                    SELECT image_id 
                    FROM news_images_relationship 
                    WHERE fk_news_id = @NewsId
                )
                DELETE FROM news_content 
                USING ImageIds
                WHERE news_content.fk_image_id = ImageIds.image_id;

                DELETE FROM images 
                USING ImageIds
                WHERE images.id = ImageIds.image_id;

                DELETE FROM news_images_relationship 
                WHERE fk_news_id = @NewsId;

                SELECT image_id FROM ImageIds;
            ";

            var deletedImageIds = new List<Guid>();
            using (var command = paramsDto.Context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = commandText;
                command.Parameters.Add(new NpgsqlParameter("@NewsId", paramsDto.Id));
                
                // Проверка на null перед использованием
                if (command.Connection == null)
                {
                    throw new InvalidOperationException("The database connection is null.");
                }

                if (command.Connection.State != System.Data.ConnectionState.Open)
                {
                    await command.Connection.OpenAsync();
                }

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        deletedImageIds.Add(reader.GetGuid(0));
                    }
                }
            }

            return deletedImageIds;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while deleting images related to the news.", ex);
        }
    }



    public async Task DeleteAllTextBlocksByNewsId(DeleteTextParamsCrudDto paramsDto)
    {
        try
        {
            var commandText = @"
                WITH ContentIds AS (
                    SELECT nc.id 
                    FROM news_content nc
                    JOIN news_relationships nr ON nc.id = nr.fk_content
                    WHERE nr.fk_news = @NewsId
                )
                DELETE FROM news_content
                WHERE id IN (SELECT id FROM ContentIds);

                DELETE FROM news_relationships
                WHERE fk_content IN (SELECT id FROM ContentIds);
            ";
            await _sessionIterator.ExecuteSqlRawAsync(paramsDto.Context, commandText,
                new NpgsqlParameter("@NewsId", paramsDto.Id));
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while deleting all text blocks.", ex);
        }
    }


    public async Task<List<Guid>> DeleteImagesByNewsIdAndBlockNumber(DeleteNewsImageParamsDto paramsDto)
    {
        try
        {
            var commandText = @"
                WITH ImageIds AS (
                    SELECT nc.fk_image_id
                    FROM news_content nc
                    JOIN news_images_relationship nir ON nc.fk_image_id = nir.image_id
                    WHERE nir.fk_news_id = @NewsId AND nc.block_number = @BlockNumber
                ),

                DeletedNewsContent AS (
                    DELETE FROM news_content
                    WHERE fk_image_id IN (SELECT fk_image_id FROM ImageIds)
                    RETURNING fk_image_id
                ),

                DeletedImages AS (
                    DELETE FROM images
                    WHERE id IN (SELECT fk_image_id FROM ImageIds)
                    RETURNING id
                ),

                DeletedNewsImagesRelationship AS (
                    DELETE FROM news_images_relationship
                    WHERE image_id IN (SELECT fk_image_id FROM ImageIds)
                    RETURNING image_id
                ),

                UpdatedNewsContent AS (
                    UPDATE news_content
                    SET block_number = block_number - 1
                    WHERE block_number > @BlockNumber
                    AND fk_image_id IN (
                        SELECT nir.image_id
                        FROM news_images_relationship nir
                        WHERE nir.fk_news_id = @NewsId
                    )
                    RETURNING fk_image_id
                )
                SELECT id FROM DeletedImages;
            ";
            var deletedImageIds = new List<Guid>();
            using (var command = paramsDto.Context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = commandText;
                command.Parameters.Add(new NpgsqlParameter("@NewsId", paramsDto.Id));
                command.Parameters.Add(new NpgsqlParameter("@BlockNumber", paramsDto.BlockNumber));
                if (command.Connection == null)
                {
                    throw new InvalidOperationException("The database connection is null.");
                }
                if (command.Connection.State != System.Data.ConnectionState.Open)
                {
                    await command.Connection.OpenAsync();
                }
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        deletedImageIds.Add(reader.GetGuid(0));
                    }
                }
            }
            return deletedImageIds;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while deleting images related to the news and block number, and shifting block numbers.", ex);
        }
    }


    public async Task DeleteTextBlockByNewsIdAndBlockNumber(DeleteTextBlockImageParamsDto paramsDto)
    {
        try
        {
            var commandText = @"
                WITH ContentIds AS (
                    SELECT nc.id 
                    FROM news_content nc
                    JOIN news_relationships nr ON nc.id = nr.fk_content
                    WHERE nr.fk_news = @NewsId AND nc.block_number = @BlockNumber
                )
                DELETE FROM news_content
                WHERE id IN (SELECT id FROM ContentIds);

                DELETE FROM news_relationships
                WHERE fk_content IN (SELECT id FROM ContentIds);

                UPDATE news_content
                SET block_number = block_number - 1
                WHERE block_number > @BlockNumber
                AND id IN (
                    SELECT fk_content
                    FROM news_relationships
                    WHERE fk_news = @NewsId
                );
            ";
            await _sessionIterator.ExecuteSqlRawAsync(paramsDto.Context, commandText,
                new NpgsqlParameter("@NewsId", paramsDto.Id),
                new NpgsqlParameter("@BlockNumber", paramsDto.BlockNumber));
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while deleting the text block and shifting block numbers.", ex);
        }
    }

    public async Task DeleteOrder(ApplicationDbContext context, Guid orderId)
    {
        try
        {
            var commandText = @"
                DO $$
                BEGIN
                    DELETE FROM order_discounts_relationship
                    WHERE order_id = @OrderId;

                    DELETE FROM order_items_relationship
                    WHERE fk_order_id = @OrderId;

                    DELETE FROM order_items
                    WHERE id IN (SELECT fk_order_item_id FROM order_items_relationship WHERE fk_order_id = @OrderId);

                    DELETE FROM orders
                    WHERE id = @OrderId;
                END;
                $$;
            ";
            var parameters = new[]
            {
                new NpgsqlParameter("@OrderId", orderId)
            };
            await _sessionIterator.ExecuteSqlRawAsync(context, commandText, parameters);
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while deleting the order.", ex);
        }
    }

    public async Task<List<Guid>> DeleteProduct(ApplicationDbContext context, Guid productId)
    {
        List<Guid> imageIds = new List<Guid>();
        try
        {
            var commandText = @"
                DO $$
                DECLARE
                    imageId UUID;
                BEGIN
                    FOR imageId IN
                        SELECT fk_image
                        FROM product_image_relationship
                        WHERE fk_product = @ProductId
                    LOOP
                        imageIds := array_append(imageIds, imageId);
                    END LOOP;

                    DELETE FROM category_relationship
                    WHERE fk_product = @ProductId;

                    DELETE FROM product_image_relationship
                    WHERE fk_product = @ProductId;

                    DELETE FROM products
                    WHERE id = @ProductId;
                END;
                $$;
            ";
            var parameters = new[]
            {
                new NpgsqlParameter("@ProductId", productId),
                new NpgsqlParameter("imageIds", imageIds)
            };
            await _sessionIterator.ExecuteSqlRawAsync(context, commandText, parameters);        
            return imageIds;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while deleting the product.", ex);
        }
    }

    public async Task DeleteProductImages(ApplicationDbContext context, Guid productId, List<Guid> imageIds)
    {
        try
        {
                var commandText = @"
                    DO $$
                    BEGIN
                        DELETE FROM images
                        WHERE id = ANY(@ImageIds::uuid[]);

                        DELETE FROM product_image_relationship
                        WHERE fk_product = @ProductId AND fk_image = ANY(@ImageIds::uuid[]);
                    END;
                    $$;
                ";
                var parameters = new[]
                {
                    new NpgsqlParameter("@ProductId", productId),
                    new NpgsqlParameter("@ImageIds", imageIds.ToArray())
                };
                await _sessionIterator.ExecuteSqlRawAsync(context, commandText, parameters);
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while deleting images for the product.", ex);
        }
    }
}



