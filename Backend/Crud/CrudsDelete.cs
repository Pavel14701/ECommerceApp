using Microsoft.EntityFrameworkCore;
using Npgsql;

public class DeleteNewsParamsCrudDto : NewsReadResultDto
{}

public class DeleteImageParamsCrudDto : DeleteNewsParamsCrudDto
{}

public class DeleteTextParamsCrudDto : DeleteNewsParamsCrudDto
{}

public class DeleteNewsImageParamsDto : DeleteNewsParamsCrudDto
{
    public required int BlockNumber { get; set;}
}

public class DeleteTextBlockImageParamsDto : DeleteNewsImageParamsDto
{}

public class DeleteOrderParamsCrudDto : DeleteNewsParamsCrudDto
{}

public class DeleteProductParamsCrudDto : DeleteOrderParamsCrudDto
{} 

public class DeleteImagesProductParamsCrudDto : DeleteOrderParamsCrudDto
{
    public required List<Guid> ImageIds { get; set; }
}




public interface IDeleteCrud
{
    Task DeleteNews(DeleteNewsParamsCrudDto paramsDto);
    Task<List<Guid>> DeleteAllImagesByNewsId(DeleteImageParamsCrudDto paramsDto);
    Task DeleteAllTextBlocksByNewsId(DeleteTextParamsCrudDto paramsDto);
    Task<List<Guid>> DeleteImagesByNewsIdAndBlockNumber(DeleteNewsImageParamsDto paramsDto);
    Task DeleteTextBlockByNewsIdAndBlockNumber(DeleteTextBlockImageParamsDto paramsDto);
    Task DeleteOrder(DeleteOrderParamsCrudDto paramsDto);
    Task<List<Guid>> DeleteProduct(DeleteProductParamsCrudDto paramsDto);
    Task DeleteProductImages(DeleteImagesProductParamsCrudDto paramsDto);

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
            await _sessionIterator.ExecuteAsync(async context =>
            {
                await context.Database.ExecuteSqlRawAsync
                (
                    commandText, new NpgsqlParameter("@NewsId", paramsDto.Id)
                );
            });
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
            var sql = @"
                WITH ImageIds AS (
                    SELECT image_id 
                    FROM news_images_relationship 
                    WHERE fk_news_id = @NewsId
                ),
                DeletedNewsContent AS (
                    DELETE FROM news_content
                    WHERE fk_image_id IN (SELECT image_id FROM ImageIds)
                    RETURNING fk_image_id
                ),
                DeletedImages AS (
                    DELETE FROM images
                    WHERE id IN (SELECT image_id FROM ImageIds)
                    RETURNING id
                ),
                DeletedNewsImagesRelationship AS (
                    DELETE FROM news_images_relationship
                    WHERE image_id IN (SELECT image_id FROM ImageIds)
                    RETURNING image_id
                )
                SELECT image_id FROM ImageIds;
            ";
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@NewsId", paramsDto.Id)
            };
            List<Guid> deletedImageIds = new List<Guid>();
            await _sessionIterator.ExecuteAsync(async context =>
            {
                var result = await context.Set<ImageIdResult>().FromSqlRaw(sql, parameters.ToArray()).ToListAsync();
                deletedImageIds = result.Select(r => r.Id).ToList();
            });
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
            await _sessionIterator.ExecuteAsync(async context =>
            {
                await context.Database.ExecuteSqlRawAsync
                (
                    commandText, new NpgsqlParameter("@NewsId", paramsDto.Id)
                );
            });
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
            var sql = @"
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
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@NewsId", paramsDto.Id),
                new NpgsqlParameter("@BlockNumber", paramsDto.BlockNumber)
            };
            List<Guid> deletedImageIds = new List<Guid>();
            await _sessionIterator.ExecuteAsync(async context =>
            {
                var result = await context.Set<ImageIdResult>().FromSqlRaw(sql, parameters).ToListAsync();
                deletedImageIds = result.Select(r => r.Id).ToList();
            });
            return deletedImageIds;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while deleting images related to the news and block number, and shifting block numbers.", ex);
        }
    }

    private class ImageIdResult
    {
        public Guid Id { get; set; }
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
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@NewsId", paramsDto.Id),
                new NpgsqlParameter("@BlockNumber", paramsDto.BlockNumber)
            };
            await _sessionIterator.ExecuteAsync(async context =>
            {
                await context.Database.ExecuteSqlRawAsync(commandText, parameters);
            });
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while deleting the text block and shifting block numbers.", ex);
        }
    }

    public async Task DeleteOrder(DeleteOrderParamsCrudDto paramsDto)
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
            await _sessionIterator.ExecuteAsync(async context =>
            {
                await context.Database.ExecuteSqlRawAsync(commandText, new NpgsqlParameter("@OrderId", paramsDto.Id));
            });
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while deleting the order.", ex);
        }
    }

    public async Task<List<Guid>> DeleteProduct(DeleteProductParamsCrudDto paramsDto)
    {
        try
        {
            var sql = @"
                WITH ImageIds AS (
                    SELECT fk_image AS ImageId
                    FROM product_image_relationship
                    WHERE fk_product = @ProductId
                ),
                DeletedCategoryRelationship AS (
                    DELETE FROM category_relationship
                    WHERE fk_product = @ProductId
                    RETURNING fk_product
                ),
                DeletedProductImageRelationship AS (
                    DELETE FROM product_image_relationship
                    WHERE fk_product = @ProductId
                    RETURNING fk_image AS ImageId
                ),
                DeletedProducts AS (
                    DELETE FROM products
                    WHERE id = @ProductId
                    RETURNING id
                )
                SELECT ImageId FROM ImageIds;
            ";
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@ProductId", paramsDto.Id)
            };
            List<Guid> imageIds = new List<Guid>();
            await _sessionIterator.ExecuteAsync(async context =>
            {
                var result = await context.Set<ImageIdResult>().FromSqlRaw(sql, parameters).ToListAsync();
                imageIds = result.Select(r => r.Id).ToList();
            });
            return imageIds;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while deleting the product.", ex);
        }
    }

    public async Task DeleteProductImages(DeleteImagesProductParamsCrudDto paramsDto)
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
                var parameters = new List<NpgsqlParameter>
                {
                    new NpgsqlParameter("@ProductId", paramsDto.Id),
                    new NpgsqlParameter("@ImageIds", paramsDto.ImageIds.ToArray())
                };
                await _sessionIterator.ExecuteAsync(async context =>
                {
                    await context.Database.ExecuteSqlRawAsync(commandText, parameters);
                });
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while deleting images for the product.", ex);
        }
    }
}



