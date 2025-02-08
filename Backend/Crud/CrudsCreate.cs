using Microsoft.EntityFrameworkCore;
using Npgsql;

public abstract class AbsId
{
    public required Guid Id { get; set; }
}

public class CreateProductDto : AbsId
{
    public string Name { get; set; } = string.Empty;
    public Guid RelationshipId { get; set; }
    public Guid CategoryId { get; set; }
    public Guid SubcategoryId { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class CreateOrderParamsCrudDto : AbsId
{
    public required Guid UserId { get; set; }
    public required DateTime OrderDate { get; set; }
    public required decimal TotalAmount { get; set; }
    public required List<Product> OrderItems { get; set; }
}

public abstract class AbsImage : AbsId
{
    public required string AltText { get; set; }
    public required string ImageUrl { get; set; }
}

public class CreateNewsCrudDto : AbsImage
{
    public required Guid ImageId { get; set; }
    public required Guid ContentIdImage { get; set; }
    public required Guid ContentIdText { get; set; }
    public required string NewsTitle { get; set; }

    public required string TextContent { get; set; }
}

public class CreateImageCrudDto : AbsImage
{
    public required Guid RelationshipId { get; set; }

    public required Guid NewsId { get; set; }
}

public abstract class AbsRelationship : AbsId
{
    public required Guid RelationshipId { get; set; }
    public required int BlockNumber { get; set; }
}

public class CreateContentImageCrudDto : AbsRelationship
{
    public required Guid ImageId { get; set; }
}

public class CreateContentTextCrudDto : AbsRelationship
{
    public required string TextContent { get; set; }

    public Guid? NewsId { get; set; }
}



public interface ICreateCrud
{
    Task AddNews(CreateNewsCrudDto paramsDto);
    Task AddImage(CreateImageCrudDto paramsDto);
    Task AddNewsContentImage(CreateContentImageCrudDto paramsDto);
    Task AddNewsContentText(CreateContentTextCrudDto paramsDto);
    Task CreateOrder(CreateOrderParamsCrudDto paramsDto);
    Task CreateProduct(CreateProductDto paramsDto);
}



public class CreateCrud : ICreateCrud
{
    private readonly SessionIterator _sessionIterator;

    public CreateCrud(SessionIterator sessionIterator)
    {
        _sessionIterator = sessionIterator;
    }


    public async Task AddNews(CreateNewsCrudDto paramsDto)
    {
        await _sessionIterator.ExecuteAsync(async context =>
        {
            var sql = @"
                DO $$
                DECLARE
                    news_id UUID := @newsId;
                    image_id UUID := @imageId;
                    content_id_text UUID := @contentIdText;
                    content_id_image UUID := @contentIdImage;
                BEGIN
                    -- Create news
                    INSERT INTO news (id, news_title, publish_datetime, update_datetime)
                    VALUES (news_id, @news_title, NOW(), NOW());

                    -- Add image as the first block
                    INSERT INTO images (id, image_url, alt)
                    VALUES (image_id, @image_url, @alt);
                    INSERT INTO news_content (id, text, block_number, fk_news_id)
                    VALUES (content_id_image, NULL, 1, news_id);
                    INSERT INTO news_images_relationship (id, image_id, fk_news_id)
                    VALUES (gen_random_uuid(), image_id, news_id);

                    -- Add text as the second block
                    INSERT INTO news_content (id, text, block_number, fk_news_id)
                    VALUES (content_id_text, @text, 2, news_id);

                    -- Create relationships between news and content
                    INSERT INTO news_relationships (id, fk_content, fk_news)
                    VALUES (gen_random_uuid(), content_id_image, news_id);
                    INSERT INTO news_relationships (id, fk_content, fk_news)
                    VALUES (gen_random_uuid(), content_id_text, news_id);

                    -- Return the id of the added image
                    RETURN image_id;
                END;
                $$;
            ";
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("newsId", paramsDto.Id),
                new NpgsqlParameter("news_title", paramsDto.NewsTitle),
                new NpgsqlParameter("imageId", paramsDto.ImageId),
                new NpgsqlParameter("contentIdImage", paramsDto.ContentIdImage),
                new NpgsqlParameter("contentIdText", paramsDto.ContentIdText),
                new NpgsqlParameter("news_title", paramsDto.NewsTitle),
                new NpgsqlParameter("image_url", paramsDto.ImageUrl),
                new NpgsqlParameter("alt", paramsDto.AltText),
                new NpgsqlParameter("text", paramsDto.TextContent)
            };
            await context.Database.ExecuteSqlRawAsync(sql, parameters);
        });
    }

    public async Task AddImage(CreateImageCrudDto paramsDto)
    {
        await _sessionIterator.ExecuteAsync(async context =>
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
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@id", paramsDto.Id),
                new NpgsqlParameter("@image_url", paramsDto.ImageUrl),
                new NpgsqlParameter("@alt", paramsDto.AltText),
                new NpgsqlParameter("@rel_id", paramsDto.RelationshipId),
                new NpgsqlParameter("@fk_news_id", paramsDto.NewsId)
            };
            await context.Database.ExecuteSqlRawAsync(commandTextImage, parameters);
        });
    }

    public async Task AddNewsContentImage(CreateContentImageCrudDto paramsDto)
    {
        await _sessionIterator.ExecuteAsync(async context =>
        {
            var commandText = @"
                DO $$
                BEGIN
                    INSERT INTO news_content (id, text, block_number)
                    VALUES (@id, NULL, @block_number);
                    INSERT INTO news_relationships (id, fk_content, fk_image_id, block_number)
                    VALUES (@rel_id, @content_id, @image_id, @block_number);
                END;
                $$;
            ";
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@id", paramsDto.Id),
                new NpgsqlParameter("@block_number", paramsDto.BlockNumber),
                new NpgsqlParameter("@rel_id", paramsDto.RelationshipId),
                new NpgsqlParameter("@content_id", paramsDto.Id),
                new NpgsqlParameter("@image_id", paramsDto.ImageId)
            };
            await context.Database.ExecuteSqlRawAsync(commandText, parameters);
        });
    }



    public async Task AddNewsContentText(CreateContentTextCrudDto paramsDto)
    {
        await _sessionIterator.ExecuteAsync(async context =>
        {
            var commandText = @"
                DO $$
                BEGIN
                    IF (SELECT COUNT(*) FROM news_content 
                        WHERE block_number = @block_number 
                        AND text IS NOT NULL) > 0 THEN
                        RAISE EXCEPTION 'Content with block number % already exists.', @block_number;
                    END IF;
                    INSERT INTO news_content (id, text, block_number)
                    VALUES (@id, @text, @block_number);
                    INSERT INTO news_relationships (id, fk_content, fk_news_id)
                    VALUES (@rel_id, @content_id, @news_id);
                END;
                $$;
            ";
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@id", paramsDto.Id),
                new NpgsqlParameter("@text", paramsDto.TextContent),
                new NpgsqlParameter("@block_number", paramsDto.BlockNumber),
                new NpgsqlParameter("@rel_id", paramsDto.RelationshipId),
                new NpgsqlParameter("@content_id", paramsDto.Id),
                new NpgsqlParameter("@news_id", paramsDto.NewsId)
            };
            await context.Database.ExecuteSqlRawAsync(commandText, parameters);
        });
    }


    public async Task CreateOrder(CreateOrderParamsCrudDto paramsDto)
    {
        await _sessionIterator.ExecuteAsync(async context =>
        {
            var commandText = @"
                WITH order_insert AS (
                    INSERT INTO orders (id, user_id, order_date, total_amount)
                    VALUES (@OrderId, @UserId, @OrderDate, @TotalAmount)
                    RETURNING id
                ), 
                product_update AS (
                    UPDATE products
                    SET stock = stock - data.quantity
                    FROM (
                        SELECT 
                            (jsonb_array_elements(@OrderItems)->>'ProductId')::UUID AS product_id,
                            (jsonb_array_elements(@OrderItems)->>'Quantity')::INT AS quantity
                        FROM orders
                        WHERE id = @OrderId
                    ) AS data
                    WHERE products.id = data.product_id
                    RETURNING products.id AS product_id
                ),
                order_items_insert AS (
                    INSERT INTO order_items (id, quantity, unit_price)
                    SELECT 
                        uuid_generate_v4(), 
                        (jsonb_array_elements(@OrderItems)->>'Quantity')::INT,
                        (jsonb_array_elements(@OrderItems)->>'UnitPrice')::NUMERIC
                    FROM orders
                    WHERE id = @OrderId
                    RETURNING id
                )
                INSERT INTO order_items_relationship (id, fk_order_id, fk_order_item_id)
                SELECT
                    uuid_generate_v4(),
                    @OrderId,
                    order_items_insert.id
                FROM order_items_insert;
            ";
            var parameters = new[]
            {
                new NpgsqlParameter("@OrderId", paramsDto.Id),
                new NpgsqlParameter("@UserId", paramsDto.UserId),
                new NpgsqlParameter("@OrderDate", paramsDto.OrderDate),
                new NpgsqlParameter("@TotalAmount",paramsDto.TotalAmount),
                new NpgsqlParameter("@OrderItems", Newtonsoft.Json.JsonConvert.SerializeObject(paramsDto.OrderItems))
            };
        await context.Database.ExecuteSqlRawAsync(commandText, parameters);
        });
    }



    public async Task CreateProduct(CreateProductDto paramsDto)
    {
        await _sessionIterator.ExecuteAsync(async context =>
        {
            var commandText = @"
                DO $$
                BEGIN
                    INSERT INTO products (
                        id, name, fk_category, fk_subcategory, price, stock, description
                    )
                    VALUES (
                        @ProductId, @Name, @CategoryId, @SubcategoryId, @Price, @Stock, @Description
                    );
                    INSERT INTO category_relationship (
                        id, fk_category, fk_subcategory, fk_product
                    )
                    VALUES (
                        @RelationshipId, @CategoryId, @SubcategoryId, @ProductId
                    );
                END;
                $$;
            ";
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@ProductId", paramsDto.Id),
                new NpgsqlParameter("@Name", paramsDto.Name),
                new NpgsqlParameter("@CategoryId", paramsDto.CategoryId),
                new NpgsqlParameter("@SubcategoryId", paramsDto.SubcategoryId),
                new NpgsqlParameter("@Price", paramsDto.Price),
                new NpgsqlParameter("@Stock", paramsDto.Stock),
                new NpgsqlParameter("@Description", paramsDto.Description),
                new NpgsqlParameter("@RelationshipId", paramsDto.RelationshipId)
            };
            await context.Database.ExecuteSqlRawAsync(commandText, parameters);
        });
    }
}