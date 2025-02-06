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

public class CreateProductDto
{
    public required ApplicationDbContext Context { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Guid SubcategoryId { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Description { get; set; } = string.Empty;
}




public interface ICreateCrud
{
    Task AddNews(CreateNewsCrudDto news);
    Task AddImage(CreateImageCrudDto image);
    Task AddNewsContentImage(CreateContentImageCrudDto image);
    Task AddNewsContentText(CreateContentTextCrudDto text);
    Task CreateOrder(ApplicationDbContext context, Order order);
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
        var commandText = @"
            DO $$
            BEGIN
                INSERT INTO news_content (id, text, block_number)
                VALUES (@id, @text, @block_number);

                INSERT INTO news_relationships (id, fk_content, fk_image_id, block_number)
                VALUES (@rel_id, @content_id, @image_id, @block_number);
            END;
            $$;
        ";
        var parameters = new[]
        {
            new NpgsqlParameter("@id", image.Id),
            new NpgsqlParameter("@text", DBNull.Value),
            new NpgsqlParameter("@block_number", image.BlockNumber),
            new NpgsqlParameter("@rel_id", Guid.NewGuid()),
            new NpgsqlParameter("@content_id", image.Id),
            new NpgsqlParameter("@image_id", image.ImageId)
        };

        await _sessionIterator.ExecuteSqlRawAsync(image.Context, commandText, parameters);
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
            var parameters = new[]
            {
                new NpgsqlParameter("@id", text.Id),
                new NpgsqlParameter("@text", text.TextContent),
                new NpgsqlParameter("@block_number", text.BlockNumber),
                new NpgsqlParameter("@rel_id", Guid.NewGuid()),
                new NpgsqlParameter("@content_id", text.Id),
                new NpgsqlParameter("@news_id", text.NewsId)
            };
            await _sessionIterator.ExecuteSqlRawAsync(text.Context, commandText, parameters);
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while adding news content text.", ex);
        }
    }


    public async Task CreateOrder(ApplicationDbContext context, Order order)
    {
        try
        {
            order.OrderDate = DateTime.UtcNow;
            order.CalculateTotalAmount();
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
                new NpgsqlParameter("@OrderId", order.Id),
                new NpgsqlParameter("@UserId", order.UserId),
                new NpgsqlParameter("@OrderDate", order.OrderDate),
                new NpgsqlParameter("@TotalAmount", order.TotalAmount),
                new NpgsqlParameter("@OrderItems", Newtonsoft.Json.JsonConvert.SerializeObject(order.OrderItems))
            };
            await _sessionIterator.ExecuteSqlRawAsync(context, commandText, parameters);
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while creation new order", ex);
        }   
    }


    public async Task CreateProductAsync(CreateProductDto productDto)
    {
        try
        {
            var productId = Guid.NewGuid();
            var relationshipId = Guid.NewGuid();
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
            var parameters = new[]
            {
                new NpgsqlParameter("@ProductId", productId),
                new NpgsqlParameter("@Name", productDto.Name),
                new NpgsqlParameter("@CategoryId", productDto.CategoryId),
                new NpgsqlParameter("@SubcategoryId", productDto.SubcategoryId),
                new NpgsqlParameter("@Price", productDto.Price),
                new NpgsqlParameter("@Stock", productDto.Stock),
                new NpgsqlParameter("@Description", productDto.Description),
                new NpgsqlParameter("@RelationshipId", relationshipId)
            };
            await _sessionIterator.ExecuteSqlRawAsync(productDto.Context, commandText, parameters);
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while creating the product.", ex);
        }
    }

}