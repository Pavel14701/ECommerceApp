using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Newtonsoft.Json;


public abstract class AbsId
{
    public required Guid Id { get; set; }
}

public class CreateProductDto : AbsId
{
    public required string Name { get; set; } = string.Empty;
    public required Guid RelationshipId { get; set; }
    public required Guid CategoryId { get; set; }
    public required Guid SubcategoryId { get; set; }
    public required decimal Price { get; set; }
    public required int Stock { get; set; }
    public required string Description { get; set; } = string.Empty;
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

public class OrderItemInfo
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public abstract class OrderAbs : AbsId
{
    public List<OrderItemInfo> Items { get; set; } = new List<OrderItemInfo>();
}

public class CreateOrderParamsCrudDto : OrderAbs
{
    public Guid UserId { get; set; }
    public DateTime OrderDate { get; set; }
    public string? DiscountCode { get; set; }
}

public class OrderCreationResult : OrderAbs
{
    public decimal TotalAmount { get; set; }
}

public class CreateImageDto
{
    public required Guid Id { get; set; }
    public required string ImageUrl { get; set; }
    public required string Alt { get; set; }
}

public class AddProductImage
{
    public required Guid ProductId { get; set; }
    public required List<CreateImageDto> Images{ get; set; }
}

public interface ICreateCrud
{
    Task AddNews(CreateNewsCrudDto paramsDto);
    Task AddImagesToNews(CreateImageCrudDto paramsDto);
    Task AddNewsContentImage(CreateContentImageCrudDto paramsDto);
    Task AddNewsContentText(CreateContentTextCrudDto paramsDto);
    Task<OrderCreationResult> CreateOrder(CreateOrderParamsCrudDto paramsDto);
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

    public async Task AddImagesToNews(CreateImageCrudDto paramsDto)
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

public async Task<OrderCreationResult> CreateOrder(CreateOrderParamsCrudDto paramsDto)
{
    Guid orderId = Guid.Empty;
    List<OrderItemInfo> orderItems = new List<OrderItemInfo>();
    decimal totalAmount = 0;
    await _sessionIterator.ExecuteAsync(async context =>
    {
        var commandText = @"
            DECLARE @OrderId UUID = @OrderIdParam;

            WITH order_items_data AS (
                SELECT 
                    data.product_id,
                    data.quantity,
                    p.price AS unit_price
                FROM
                    (SELECT 
                        (jsonb_array_elements(@OrderItems)->>'ProductId')::UUID AS product_id,
                        (jsonb_array_elements(@OrderItems)->>'Quantity')::INT AS quantity
                     ) AS data
                JOIN products p ON p.id = data.product_id
            ),
            order_insert AS (
                INSERT INTO orders (id, user_id, order_date, total_amount)
                VALUES (@OrderId, @UserId, @OrderDate, 0)
                RETURNING id
            ), 
            product_update AS (
                UPDATE products
                SET stock = stock - data.quantity
                FROM order_items_data AS data
                WHERE products.id = data.product_id
                RETURNING products.id AS product_id
            ),
            order_items_insert AS (
                INSERT INTO order_items (id, quantity, unit_price)
                SELECT 
                    uuid_generate_v4(), 
                    data.quantity,
                    data.unit_price
                FROM order_items_data AS data
                RETURNING id, data.product_id, data.quantity
            ),
            order_items_relationship_insert AS (
                INSERT INTO order_items_relationship (id, fk_order_id, fk_order_item_id)
                SELECT
                    uuid_generate_v4(),
                    @OrderId,
                    order_items_insert.id
                FROM order_items_insert
                RETURNING id
            ),
            OrderTotal AS (
                SELECT 
                    @OrderId AS OrderId,
                    SUM(data.quantity * data.unit_price) AS TotalOrderItemsAmount
                FROM order_items_data AS data
            ),
            ApplicableDiscount AS (
                SELECT 
                    @OrderId AS OrderId,
                    CASE
                        WHEN d.fk_category IS NULL THEN d.amount
                        ELSE SUM(CASE WHEN p.category_id = d.fk_category THEN d.amount ELSE 0 END)
                    END AS TotalDiscountAmount
                FROM 
                    orders o
                LEFT JOIN 
                    order_discounts_relationship odr ON o.id = odr.order_id
                LEFT JOIN 
                    discounts d ON odr.discount_id = d.id
                LEFT JOIN 
                    order_items_relationship oir ON o.id = oir.fk_order_id
                LEFT JOIN 
                    order_items oi ON oir.fk_order_item_id = oi.id
                LEFT JOIN 
                    products p ON oi.product_id = p.id
                WHERE 
                    o.id = @OrderId
                    AND d.code = @DiscountCode
                GROUP BY 
                    o.id, d.amount, d.fk_category
            )

            UPDATE orders
            SET total_amount = (
                SELECT 
                    (ot.TotalOrderItemsAmount - COALESCE(ad.TotalDiscountAmount, 0)) AS FinalTotalAmount
                FROM 
                    OrderTotal ot
                LEFT JOIN 
                    ApplicableDiscount ad ON ot.OrderId = ad.OrderId
                WHERE 
                    ot.OrderId = @OrderId
            )
            WHERE id = @OrderId;

            SELECT 
                @OrderId AS OrderId,
                order_items_insert.product_id,
                order_items_insert.quantity,
                (
                    SELECT 
                        (ot.TotalOrderItemsAmount - COALESCE(ad.TotalDiscountAmount, 0)) AS FinalTotalAmount
                    FROM 
                        OrderTotal ot
                    LEFT JOIN 
                        ApplicableDiscount ad ON ot.OrderId = ad.OrderId
                    WHERE 
                        ot.OrderId = @OrderId
                ) AS TotalAmount
            FROM 
                order_items_insert;
        ";
        var parameters = new[]
        {
            new NpgsqlParameter("@OrderId", paramsDto.Id),
            new NpgsqlParameter("@OrderIdParam", paramsDto.Id),
            new NpgsqlParameter("@UserId", paramsDto.UserId),
            new NpgsqlParameter("@OrderDate", paramsDto.OrderDate),
            new NpgsqlParameter("@OrderItems", JsonConvert.SerializeObject(paramsDto.Items)),
            new NpgsqlParameter("@DiscountCode", paramsDto.DiscountCode)
        };
        var products = new List<OrderItemInfo>();
        using (var command = context.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = commandText;
            command.Parameters.AddRange(parameters);
            await context.Database.OpenConnectionAsync();
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    products.Add(new OrderItemInfo
                    {
                        ProductId = reader.GetGuid(1),
                        Quantity = reader.GetInt32(2),
                        UnitPrice = reader.GetDecimal(3)
                    });
                }
            }
            await context.Database.CloseConnectionAsync();
        }
        foreach (var product in products)
        {
            orderId = product.ProductId;
            totalAmount += product.Quantity * product.UnitPrice;
            orderItems.Add(new OrderItemInfo
            {
                ProductId = product.ProductId,
                Quantity = product.Quantity,
                UnitPrice = product.UnitPrice
            });
        }
    });
    return new OrderCreationResult
    {
        Id = orderId,
        Items = orderItems,
        TotalAmount = totalAmount
    };
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

    public async Task AddImagesToProduct(AddProductImage paramsDto)
    {
        try{
        await _sessionIterator.ExecuteAsync(async context =>
        {
            var commandText = @"
                DO $$
                DECLARE
                    _image_ids UUID[] := ARRAY[];
                    _image_urls TEXT[] := ARRAY[];
                    _image_alts TEXT[] := ARRAY[];
                BEGIN
                    IF (SELECT COUNT(*) FROM products WHERE id = @ProductId) = 0 THEN
                        RAISE EXCEPTION 'Product not found';
                    END IF;

                    SELECT 
                        array_agg(image.Id), 
                        array_agg(image.ImageUrl), 
                        array_agg(image.Alt)
                    INTO
                        _image_ids,
                        _image_urls,
                        _image_alts
                    FROM 
                        unnest(@Images) AS image(Id, ImageUrl, Alt);

                    INSERT INTO images (id, image_url, alt)
                    SELECT unnest(_image_ids), unnest(_image_urls), unnest(_image_alts);

                    INSERT INTO product_image_relationship (id, image_id, fk_product_id)
                    SELECT uuid_generate_v4(), unnest(_image_ids), @ProductId;
                END $$;
            ";
            var imageArray = paramsDto.Images.Select(image => new
            {
                image.Id,
                image.ImageUrl,
                image.Alt
            }).ToArray();
            var parameters = new[]
            {
                new NpgsqlParameter("@ProductId", paramsDto.ProductId),
                new NpgsqlParameter("@Images", imageArray)
            };
            await context.Database.ExecuteSqlRawAsync(commandText, parameters);
        });
        }catch{throw;}   
    }
}
