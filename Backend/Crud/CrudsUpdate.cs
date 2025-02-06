using Microsoft.EntityFrameworkCore;
using Npgsql;

public abstract class UpdateNewsParamsBase
{
    public required ApplicationDbContext Context { get; set; }
    public required Guid NewsId { get; set; }
}

public class UpdateNewsTitleParamsDto : UpdateNewsParamsBase
{
    public required string NewTitle { get; set; }
}

public class UpdateNewsImagesParamsDto : UpdateNewsParamsBase
{
    public required Dictionary<Guid, string> NewImageUrls { get; set; }
    public required int BlockNumber { get; set; }
}

public class UpdateNewsTextParamsDto : UpdateNewsParamsBase
{
    public required string NewText { get; set; }
    public required int BlockNumber { get; set; }
}


public interface IUpdateCrud
{
    Task UpdateNewsTitle(UpdateNewsTitleParamsDto paramsDto);
    Task UpdateImagesByBlockNumber(UpdateNewsImagesParamsDto paramsDto);
    Task UpdateTextBlock(UpdateNewsTextParamsDto paramsDto);
    Task UpdateProductStock(ApplicationDbContext context, Guid productId, int quantity);
    Task ApplyDiscountToOrder(ApplicationDbContext context, Guid orderId, Guid discountId);
}

public class UpdateCrud : IUpdateCrud
{
    public async Task UpdateNewsTitle(UpdateNewsTitleParamsDto paramsDto)
    {
        try
        {
            var commandText = @"
                UPDATE news
                SET news_title = @NewTitle,
                    update_datetime = NOW()
                WHERE id = @NewsId;
            ";
            using (var command = paramsDto.Context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = commandText;
                command.Parameters.Add(new NpgsqlParameter("@NewTitle", paramsDto.NewTitle));
                command.Parameters.Add(new NpgsqlParameter("@NewsId", paramsDto.NewsId));
                if (command.Connection == null)
                {
                    throw new InvalidOperationException("The database connection is null.");
                }
                if (command.Connection.State != System.Data.ConnectionState.Open)
                {
                    await command.Connection.OpenAsync();
                }
                await command.ExecuteNonQueryAsync();
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task UpdateImagesByBlockNumber(UpdateNewsImagesParamsDto paramsDto)
    {
        try
        {
            var commandText = @"
                UPDATE images
                SET image_url = CASE
            ";
            foreach (var imageUrl in paramsDto.NewImageUrls)
            {
                commandText += $@"
                    WHEN image_id = '{imageUrl.Key}' THEN '{imageUrl.Value}'";
            }
            commandText += @"
                    ELSE image_url
                END
                WHERE image_id IN (
                    SELECT nir.image_id
                    FROM news_images_relationship nir
                    JOIN news_content nc ON nir.fk_news_id = nc.fk_news_id
                    WHERE nc.fk_news_id = @NewsId AND nc.block_number = @BlockNumber
                );

                UPDATE news
                SET update_datetime = NOW()
                WHERE id = @NewsId;
            ";
            using (var command = paramsDto.Context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = commandText;
                command.Parameters.Add(new NpgsqlParameter("@NewsId", paramsDto.NewsId));
                command.Parameters.Add(new NpgsqlParameter("@BlockNumber", paramsDto.BlockNumber));
                if (command.Connection == null)
                {
                    throw new InvalidOperationException("The database connection is null.");
                }
                if (command.Connection.State != System.Data.ConnectionState.Open)
                {
                    await command.Connection.OpenAsync();
                }
                await command.ExecuteNonQueryAsync();
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task UpdateTextBlock(UpdateNewsTextParamsDto paramsDto)
    {
        try
        {
            var commandText = @"
                WITH updated_content AS (
                    UPDATE news_content
                    SET text = @NewText
                    WHERE fk_news_id = @NewsId AND block_number = @BlockNumber
                    RETURNING fk_news_id
                )
                UPDATE news
                SET update_datetime = NOW()
                FROM updated_content
                WHERE news.id = updated_content.fk_news_id;
            ";
            using (var command = paramsDto.Context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = commandText;
                command.Parameters.Add(new NpgsqlParameter("@NewText", paramsDto.NewText));
                command.Parameters.Add(new NpgsqlParameter("@NewsId", paramsDto.NewsId));
                command.Parameters.Add(new NpgsqlParameter("@BlockNumber", paramsDto.BlockNumber));
                if (command.Connection == null)
                {
                    throw new InvalidOperationException("The database connection is null.");
                }
                if (command.Connection.State != System.Data.ConnectionState.Open)
                {
                    await command.Connection.OpenAsync();
                }
                await command.ExecuteNonQueryAsync();
            }
        }
        catch (Exception)
        {
            throw;
        }
    }




    public async Task UpdateProductStock(ApplicationDbContext context, Guid productId, int quantity)
    {
        try
        {
            var commandText = @"
                UPDATE products 
                SET stock = stock - @Quantity 
                WHERE id = @ProductId
            ";
            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = commandText;
                command.Parameters.Add(new NpgsqlParameter("@Quantity", quantity));
                command.Parameters.Add(new NpgsqlParameter("@ProductId", productId));
                if (command.Connection == null)
                {
                    throw new InvalidOperationException("The database connection is null.");
                }
                if (command.Connection.State != System.Data.ConnectionState.Open)
                {
                    await command.Connection.OpenAsync();
                }
                await command.ExecuteNonQueryAsync();
            }
        }
        catch (Exception)
        {
            throw;
        }
    }


    public async Task ApplyDiscountToOrder(ApplicationDbContext context, Guid orderId, Guid discountId)
    {
        try
        {
            var commandText = @"
                DO $$
                BEGIN
                    IF (SELECT COUNT(*) FROM discounts WHERE id = @DiscountId) = 0 THEN
                        RAISE EXCEPTION 'Discount % does not exist.', @DiscountId;
                    END IF;

                    INSERT INTO order_discounts_relationship (id, order_id, discount_id)
                    VALUES (@RelId, @OrderId, @DiscountId);

                    UPDATE orders
                    SET total_amount = total_amount - (SELECT amount FROM discounts WHERE id = @DiscountId)
                    WHERE id = @OrderId;
                END;
                $$;
            ";
            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = commandText;
                command.Parameters.Add(new NpgsqlParameter("@RelId", Guid.NewGuid()));
                command.Parameters.Add(new NpgsqlParameter("@OrderId", orderId));
                command.Parameters.Add(new NpgsqlParameter("@DiscountId", discountId));
                if (command.Connection == null)
                {
                    throw new InvalidOperationException("The database connection is null.");
                }
                if (command.Connection.State != System.Data.ConnectionState.Open)
                {
                    await command.Connection.OpenAsync();
                }
                await command.ExecuteNonQueryAsync();
            }
        }
        catch (Exception)
        {
            throw;
        }
    }
}

