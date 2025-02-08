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


public class UpdateProductParamsDto : UpdateProductDto
{
    public required ApplicationDbContext Context { get; set; }
}






public interface IUpdateCrud
{
    Task UpdateNewsTitle(UpdateNewsTitleParamsDto paramsDto);
    Task UpdateImagesByBlockNumber(UpdateNewsImagesParamsDto paramsDto);
    Task UpdateTextBlock(UpdateNewsTextParamsDto paramsDto);
    Task UpdateProductStock(Guid productId, int quantity);
    Task ApplyDiscountToOrder(Guid orderId, Guid discountId);
}

public class UpdateCrud : IUpdateCrud
{
    public readonly SessionIterator _sessionIterator
    public UpdateCrud(SessionIterator sessionIterator)
    {
        _sessionIterator = sessionIterator
    }

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
            
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@NewTitle", paramsDto.NewTitle),
                new NpgsqlParameter("@NewsId", paramsDto.NewsId)
            };
            await _sessionIterator.ExecuteAsync(async context =>
            {
                await context.Database.ExecuteSqlRawAsync(commandText, parameters);
            });
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
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@NewsId", paramsDto.NewsId),
                new NpgsqlParameter("@BlockNumber", paramsDto.BlockNumber)
            };
            await _sessionIterator.ExecuteAsync(async context =>
            {
                await context.Database.ExecuteSqlRawAsync(commandText, parameters);
            });
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
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@NewText", paramsDto.NewText),
                new NpgsqlParameter("@NewsId", paramsDto.NewsId),
                new NpgsqlParameter("@BlockNumber", paramsDto.BlockNumber)
            };
            await _sessionIterator.ExecuteAsync(async context =>
            {
                await context.Database.ExecuteSqlRawAsync(commandText, parameters);
            });
        }
        catch (Exception)
        {
            throw;
        }
    }




    public async Task UpdateProductStock(Guid productId, int quantity)
    {
        try
        {
            var commandText = @"
                UPDATE products 
                SET stock = stock - @Quantity 
                WHERE id = @ProductId
            ";
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@Quantity", quantity),
                new NpgsqlParameter("@ProductId", productId)
            };
            await _sessionIterator.ExecuteAsync(async context =>
            {
                await context.Database.ExecuteSqlRawAsync(commandText, parameters);
            });
        }
        catch (Exception)
        {
            throw;
        }
    }


    public async Task ApplyDiscountToOrder(Guid orderId, Guid discountId)
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
                    SET total_amount = total_amount - (S`ELECT amount FROM discounts WHERE id = @DiscountId)
                    WHERE id = @OrderId;
                END;
                $$;
            ";
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@RelId", Guid.NewGuid()),
                new NpgsqlParameter("@OrderId", orderId),
                new NpgsqlParameter("@DiscountId", discountId)
            };
            await _sessionIterator.ExecuteAsync(async context =>
            {
                await context.Database.ExecuteSqlRawAsync(commandText, parameters);
            });
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task UpdateProduct(UpdateProductParamsDto paramsDto)
    {
        try
        {
            var commandText = @"
                DO $$
                BEGIN
                    IF @Name IS NOT NULL THEN
                        UPDATE products
                        SET name = @Name
                        WHERE id = @ProductId;
                    END IF;

                    IF @Price IS NOT NULL THEN
                        UPDATE products
                        SET price = @Price
                        WHERE id = @ProductId;
                    END IF;

                    IF @Discount IS NOT NULL THEN
                        UPDATE products
                        SET discount = @Discount
                        WHERE id = @ProductId;
                    ELSE
                        UPDATE products
                        SET discount = NULL
                        WHERE id = @ProductId;
                    END IF;

                    DELETE FROM category_relationship
                    WHERE fk_product = @ProductId;
                    
                    IF @CategorySubcategoryPairs IS NOT NULL THEN
                        FOR i IN 1..array_length(@CategorySubcategoryPairs, 1) LOOP
                            INSERT INTO category_relationship (id, fk_category, fk_subcategory, fk_product)
                            VALUES (gen_random_uuid(), @CategorySubcategoryPairs[i].category, @CategorySubcategoryPairs[i].subcategory, @ProductId);
                        END LOOP;
                    END IF;
                END;
                $$;
            ";
            var categorySubcategoryPairs = paramsDto.CategorySubcategoryPairs
                .Select(pair => new { category = pair.Key, subcategory = pair.Value })
                .ToArray();
            var parameters = new[]
            {
                new NpgsqlParameter("@ProductId", paramsDto.ProductId),
                new NpgsqlParameter("@Name", paramsDto.Name ?? (object)DBNull.Value),
                new NpgsqlParameter("@Price", paramsDto.Price.HasValue ? (object)paramsDto.Price.Value : DBNull.Value),
                new NpgsqlParameter("@Discount", paramsDto.Discount.HasValue ? (object)paramsDto.Discount.Value : DBNull.Value),
                new NpgsqlParameter("@CategorySubcategoryPairs", categorySubcategoryPairs)
            };
            await _sessionIterator.ExecuteAsync(async context =>
            {
                await context.Database.ExecuteSqlRawAsync(commandText, parameters);
            });
        }   
        catch (Exception ex)
        {
            throw new Exception("An error occurred while updating the product.", ex);
        }
    }
}

