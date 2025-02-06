using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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

public class PaginationParamsDto
{
    public required int Page { get; set; } 
    public required int PageSize { get; set; }
}

public class NewsReadResultDto
{
    public required Guid Id { get; set; }

}

public class NewsPreviewDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public DateTime PublishDatetime { get; set; }
    public string? FirstBlockText { get; set; }
    public string? FirstImageUrl { get; set; }
}

public class NewsDetailsDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime PublishDatetime { get; set; }
    public Dictionary<int, object>? ContentBlocks { get; set; }
}

public class OrderDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
    public List<OrderDiscountDto> OrderDiscounts { get; set; } = new List<OrderDiscountDto>();
}

public class OrderItemDto
{
    public Guid Id { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class OrderDiscountDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class ProductPreviewDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? Discount { get; set; }
    public List<string> ImageUrls { get; set; } = new List<string>();
}


public class ProductInfoSearchDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Guid CategoryId { get; set; }
    public decimal? Discount { get; set; }
    public List<string> ImageUrls { get; set; } = new List<string>();
}




public interface IReadCrud
{
    Task<Result> CheckNews(CheckNewsCrudDto news);
    Task<NewsReadResultDto?> GetNewsIdByTitleAndDate(ReadNewsIdDto paramsDto);
    Task<List<NewsPreviewDto>> GetPaginatedNews(PaginationParamsDto paramsDto);
    Task<NewsDetailsDto> GetNewsDetailsById(Guid newsId);
    Task<User?> FindUserByUsername(string username);
    Task<User?> FindUserById(Guid userId);
    Task<OrderDto?> GetOrderById(Guid orderId);
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
            WHERE n.news_title = @Title AND n.publish_datetime = @PublishDatetime;
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

    public async Task<List<NewsPreviewDto>> GetPaginatedNews(PaginationParamsDto paramsDto)
    {
        var newsList = new List<NewsPreviewDto>();
        await _sessionIterator.ReadAsync(async context =>
        {
            var offset = (paramsDto.Page - 1) * paramsDto.PageSize;
            var commandText = @"
                WITH FirstTextBlock AS (
                    SELECT nc.fk_news_id, nc.text
                    FROM news_content nc
                    WHERE nc.block_number = 1
                ),
                FirstImage AS (
                    SELECT nir.fk_news_id, i.image_url
                    FROM news_images_relationship nir
                    JOIN images i ON nir.image_id = i.id
                    WHERE nir.image_id = (
                        SELECT MIN(nir2.image_id)
                        FROM news_images_relationship nir2
                        WHERE nir2.fk_news_id = nir.fk_news_id
                    )
                ),
                PaginatedNews AS (
                    SELECT
                        n.id,
                        n.news_title,
                        n.publish_datetime,
                        ft.text AS first_block_text,
                        fi.image_url AS first_image_url,
                        ROW_NUMBER() OVER (ORDER BY n.publish_datetime DESC) AS row_num
                    FROM news n
                    LEFT JOIN FirstTextBlock ft ON n.id = ft.fk_news_id
                    LEFT JOIN FirstImage fi ON n.id = fi.fk_news_id
                )
                SELECT
                    id,
                    news_title,
                    publish_datetime,
                    first_block_text,
                    first_image_url
                FROM PaginatedNews
                WHERE row_num BETWEEN @Offset AND @Offset + @PageSize - 1;
            ";
            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = commandText;
                command.Parameters.Add(new NpgsqlParameter("@Offset", offset));
                command.Parameters.Add(new NpgsqlParameter("@PageSize", paramsDto.PageSize));
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
                        var news = new NewsPreviewDto
                        {
                            Id = reader.GetGuid(0),
                            Title = reader.GetString(1),
                            PublishDatetime = reader.GetDateTime(2),
                            FirstBlockText = reader.IsDBNull(3) ? null : reader.GetString(3),
                            FirstImageUrl = reader.IsDBNull(4) ? null : reader.GetString(4)
                        };
                        newsList.Add(news);
                    }
                }
            }
            return newsList;
        });
        return newsList;
    }

    public async Task<NewsDetailsDto> GetNewsDetailsById(Guid newsId)
    {
        try
        {
            var newsDetails = new NewsDetailsDto();
            newsDetails.ContentBlocks = new Dictionary<int, object>();
            await _sessionIterator.ReadAsync<NewsDetailsDto>(async context =>
            {
                var commandText = @"
                    SELECT 
                        n.id,
                        n.news_title,
                        n.publish_datetime,
                        nc.block_number, 
                        nc.text, 
                        COALESCE(json_agg(jsonb_build_object('image_url', i.image_url, 'alt', i.alt)) FILTER (WHERE i.image_url IS NOT NULL), '[]') AS images
                    FROM news n
                    JOIN news_content nc ON n.id = nc.fk_news_id
                    JOIN news_images_relationship nir ON nc.fk_image_id = nir.image_id
                    JOIN images i ON nir.image_id = i.id
                    WHERE n.id = @NewsId
                    GROUP BY n.id, n.news_title, n.publish_datetime, nc.block_number, nc.text
                    ORDER BY nc.block_number;
                ";
                using (var command = context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = commandText;
                    command.Parameters.Add(new NpgsqlParameter("@NewsId", newsId));
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
                            if (newsDetails.Id == Guid.Empty)
                            {
                                newsDetails.Id = reader.GetGuid(0);
                                newsDetails.Title = reader.GetString(1);
                                newsDetails.PublishDatetime = reader.GetDateTime(2);
                            }
                            var blockNumber = reader.GetInt32(3);
                            var textContent = reader.IsDBNull(4) ? null : reader.GetString(4);
                            var imageData = reader.GetFieldValue<string[]>(5);
                            if (textContent != null)
                            {
                                newsDetails.ContentBlocks[blockNumber] = textContent;
                            }
                            else if (imageData != null && imageData.Length > 0)
                            {
                                newsDetails.ContentBlocks[blockNumber] = imageData.Select(imageJson => JsonConvert.DeserializeObject<Dictionary<string, string>>(imageJson)).ToList();
                            }
                        }
                    }
                }

                return newsDetails;
            });

            return newsDetails;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while fetching the news details.", ex);
        }
    }



    public async Task<User?> FindUserByUsername(string username)
    {
        try
        {
            return await _sessionIterator.ReadAsync(async context =>
            {
                var commandText = @"
                    SELECT id, username, password_hash, salt, email 
                    FROM users
                    WHERE username = @Username
                ";
                var parameters = new[]
                {
                    new NpgsqlParameter("@Username", username)
                };
                return await context.Users
                    .FromSqlRaw(commandText, parameters)
                    .SingleOrDefaultAsync();
            });
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while fetching the user details.", ex);
        }
    }


    public async Task<User?> FindUserById(Guid userId)
    {
        try
        {
            return await _sessionIterator.ReadAsync(async context =>
            {
                var commandText = @"
                    SELECT id, username, password_hash, salt, email 
                    FROM Users
                    WHERE id = @UserId
                ";
                var parameters = new[]
                {
                    new NpgsqlParameter("@UserId", userId)
                };
                return await context.Users
                    .FromSqlRaw(commandText, parameters)
                    .SingleOrDefaultAsync();
            });
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while fetching the user details.", ex);
        }
    }



    public async Task<Product?> GetProductById(Guid productId)
    {
        return await _sessionIterator.ReadAsync(async context =>
        {
            var commandText = @"
                SELECT * FROM Products
                WHERE Id = @ProductId
            ";
            var parameters = new[]
            {
                new NpgsqlParameter("@ProductId", productId)
            };

            return await context.Products
                .FromSqlRaw(commandText, parameters)
                .SingleOrDefaultAsync();
        });
    }


    public async Task<OrderDto?> GetOrderById(Guid orderId)
    {
        try
        {
            return await _sessionIterator.ReadAsync(async context =>
            {
                var commandText = @"
                    SELECT 
                        o.id AS OrderId,
                        o.user_id AS UserId,
                        o.order_date AS OrderDate,
                        o.total_amount AS TotalAmount,
                        oi.id AS OrderItemId,
                        oi.quantity AS Quantity,
                        oi.unit_price AS UnitPrice,
                        od.id AS OrderDiscountId,
                        d.code AS DiscountCode,
                        d.amount AS DiscountAmount
                    FROM orders o
                    LEFT JOIN order_items_relationship oir ON o.id = oir.fk_order_id
                    LEFT JOIN order_items oi ON oir.fk_order_item_id = oi.id
                    LEFT JOIN order_discounts_relationship odr ON o.id = odr.order_id
                    LEFT JOIN discounts d ON odr.discount_id = d.id
                    WHERE o.id = @OrderId;
                ";
                var parameters = new[]
                {
                    new NpgsqlParameter("@OrderId", orderId)
                };
                var orderDictionary = new Dictionary<Guid, OrderDto>();
                await using (var command = context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = commandText;
                    command.Parameters.AddRange(parameters);
                    if (command.Connection == null)
                    {
                        throw new InvalidOperationException("The database connection is null.");
                    }
                    if (command.Connection.State != System.Data.ConnectionState.Open)
                    {
                        await command.Connection.OpenAsync();
                    }
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var orderId = reader.GetGuid(0);
                            if (!orderDictionary.TryGetValue(orderId, out var order))
                            {
                                order = new OrderDto
                                {
                                    Id = orderId,
                                    UserId = reader.GetGuid(1),
                                    OrderDate = reader.GetDateTime(2),
                                    TotalAmount = reader.GetDecimal(3),
                                    OrderItems = new List<OrderItemDto>(),
                                    OrderDiscounts = new List<OrderDiscountDto>()
                                };
                                orderDictionary.Add(orderId, order);
                            }
                            if (!await reader.IsDBNullAsync(4))
                            {
                                var orderItem = new OrderItemDto
                                {
                                    Id = reader.GetGuid(4),
                                    Quantity = reader.GetInt32(5),
                                    UnitPrice = reader.GetDecimal(6)
                                };
                                order.OrderItems.Add(orderItem);
                            }
                            if (!await reader.IsDBNullAsync(7))
                            {
                                var orderDiscount = new OrderDiscountDto
                                {
                                    Id = reader.GetGuid(7),
                                    Code = reader.GetString(8),
                                    Amount = reader.GetDecimal(9)
                                };
                                order.OrderDiscounts.Add(orderDiscount);
                            }
                        }
                    }
                }
                return orderDictionary.Values.FirstOrDefault();
            });
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<IEnumerable<ProductPreviewDto>> SearchProducts(string? name, decimal? minPrice, decimal? maxPrice, bool? hasDiscount, Dictionary<Guid, List<Guid>>? categorySubcategoryDict, int offset, int limit)
    {
        try
        {
            var categoryIds = new List<Guid>();
            var subcategoryIds = new List<Guid>();
            if (categorySubcategoryDict != null)
            {
                foreach (var category in categorySubcategoryDict)
                {
                    categoryIds.Add(category.Key);
                    subcategoryIds.AddRange(category.Value);
                }
            }
            var commandText = @"
                WITH ProductData AS (
                    SELECT 
                        p.id AS ProductId,
                        p.name AS ProductName,
                        p.price AS Price,
                        d.amount AS Discount,
                        ROW_NUMBER() OVER (ORDER BY p.name) AS RowNum
                    FROM 
                        products p
                    LEFT JOIN 
                        category_relationship cr ON p.id = cr.fk_product
                    LEFT JOIN 
                        order_discounts_relationship odr ON p.id = odr.fk_product
                    LEFT JOIN 
                        discounts d ON odr.discount_id = d.id
                    WHERE 
                        (@Name IS NULL OR p.name ILIKE '%' || @Name || '%') AND
                        (@MinPrice IS NULL OR p.price >= @MinPrice) AND
                        (@MaxPrice IS NULL OR p.price <= @MaxPrice) AND
                        (@HasDiscount IS NULL OR (@HasDiscount = TRUE AND d.amount IS NOT NULL) OR (@HasDiscount = FALSE AND d.amount IS NULL)) AND
                        (cr.fk_category = ANY(@CategoryIds::uuid[]) OR @CategoryIds IS NULL) AND
                        (cr.fk_subcategory = ANY(@SubcategoryIds::uuid[]) OR @SubcategoryIds IS NULL)
                ),
                ImageData AS (
                    SELECT
                        pir.fk_product AS ProductId,
                        pi.image_url AS ImageUrl,
                        ROW_NUMBER() OVER (PARTITION BY pir.fk_product ORDER BY pi.image_url) AS ImageRowNum
                    FROM 
                        product_image_relationship pir
                    LEFT JOIN
                        images pi ON pir.fk_image = pi.id
                )
                SELECT 
                    pd.ProductId,
                    pd.ProductName,
                    pd.Price,
                    pd.Discount,
                    array_agg(i.ImageUrl) FILTER (WHERE i.ImageRowNum <= 5) AS ImageUrls
                FROM 
                    ProductData pd
                LEFT JOIN 
                    ImageData i ON pd.ProductId = i.ProductId
                WHERE 
                    pd.RowNum BETWEEN @Offset AND @Offset + @Limit - 1
                GROUP BY 
                    pd.ProductId, pd.ProductName, pd.Price, pd.Discount
                ORDER BY 
                    pd.RowNum;
            ";
            var parameters = new[]
            {
                new NpgsqlParameter("@Name", name ?? (object)DBNull.Value),
                new NpgsqlParameter("@MinPrice", minPrice ?? (object)DBNull.Value),
                new NpgsqlParameter("@MaxPrice", maxPrice ?? (object)DBNull.Value),
                new NpgsqlParameter("@HasDiscount", hasDiscount ?? (object)DBNull.Value),
                new NpgsqlParameter("@CategoryIds", categoryIds.Count > 0 ? (object)categoryIds.ToArray() : DBNull.Value),
                new NpgsqlParameter("@SubcategoryIds", subcategoryIds.Count > 0 ? (object)subcategoryIds.ToArray() : DBNull.Value),
                new NpgsqlParameter("@Offset", offset),
                new NpgsqlParameter("@Limit", limit)
            };
            return await _sessionIterator.ReadAsync(async context =>
            {
                var productList = new List<ProductPreviewDto>();
                await using (var command = context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = commandText;
                    command.Parameters.AddRange(parameters);
                    if (command.Connection == null)
                    {
                        throw new InvalidOperationException("The database connection is null.");
                    }
                    if (command.Connection.State != System.Data.ConnectionState.Open)
                    {
                        await command.Connection.OpenAsync();
                    }

                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var product = new ProductPreviewDto
                            {
                                Id = reader.GetGuid(0),
                                Name = reader.GetString(1),
                                Price = reader.GetDecimal(2),
                                Discount = reader.IsDBNull(3) ? (decimal?)null : reader.GetDecimal(3),
                                ImageUrls = reader.IsDBNull(4) ? new List<string>() : reader.GetFieldValue<List<string>>(4)
                            };
                            productList.Add(product);
                        }
                    }
                }
                return productList;
            });
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<(int TotalCount, Dictionary<Guid, ProductInfoSearchDto> Products)> SearchProductsInfo(string? name, decimal? minPrice, decimal? maxPrice, bool? hasDiscount, Dictionary<Guid, List<Guid>>? categorySubcategoryDict, int offset, int limit)
    {
        try
        {
            var categoryIds = new List<Guid>();
            var subcategoryIds = new List<Guid>();
            if (categorySubcategoryDict != null)
            {
                foreach (var category in categorySubcategoryDict)
                {
                    categoryIds.Add(category.Key);
                    subcategoryIds.AddRange(category.Value);
                }
            }
            var commandText = @"
                WITH ProductData AS (
                    SELECT 
                        p.id AS ProductId,
                        p.name AS ProductName,
                        p.price AS Price,
                        p.fk_category AS CategoryId,
                        d.amount AS Discount,
                        ROW_NUMBER() OVER (ORDER BY p.name) AS RowNum,
                        COUNT(*) OVER() AS TotalCount
                    FROM 
                        products p
                    LEFT JOIN 
                        category_relationship cr ON p.id = cr.fk_product
                    LEFT JOIN 
                        order_discounts_relationship odr ON p.id = odr.fk_product
                    LEFT JOIN 
                        discounts d ON odr.discount_id = d.id
                    WHERE 
                        (@Name IS NULL OR p.name ILIKE '%' || @Name || '%') AND
                        (@MinPrice IS NULL OR p.price >= @MinPrice) AND
                        (@MaxPrice IS NULL OR p.price <= @MaxPrice) AND
                        (@HasDiscount IS NULL OR (@HasDiscount = TRUE AND d.amount IS NOT NULL) OR (@HasDiscount = FALSE AND d.amount IS NULL)) AND
                        (cr.fk_category = ANY(@CategoryIds::uuid[]) OR @CategoryIds IS NULL) AND
                        (cr.fk_subcategory = ANY(@SubcategoryIds::uuid[]) OR @SubcategoryIds IS NULL)
                ),
                ImageData AS (
                    SELECT
                        pir.fk_product AS ProductId,
                        pi.image_url AS ImageUrl,
                        ROW_NUMBER() OVER (PARTITION BY pir.fk_product ORDER BY pi.image_url) AS ImageRowNum
                    FROM 
                        product_image_relationship pir
                    LEFT JOIN
                        images pi ON pir.fk_image = pi.id
                )
                SELECT 
                    pd.ProductId,
                    pd.ProductName,
                    pd.Price,
                    pd.CategoryId,
                    pd.Discount,
                    pd.TotalCount,
                    array_agg(i.ImageUrl) FILTER (WHERE i.ImageRowNum <= 5) AS ImageUrls
                FROM 
                    ProductData pd
                LEFT JOIN 
                    ImageData i ON pd.ProductId = i.ProductId
                WHERE 
                    pd.RowNum BETWEEN @Offset AND @Offset + @Limit - 1
                GROUP BY 
                    pd.ProductId, pd.ProductName, pd.Price, pd.CategoryId, pd.Discount, pd.TotalCount
                ORDER BY 
                    pd.RowNum;
            ";
            var parameters = new[]
            {
                new NpgsqlParameter("@Name", name ?? (object)DBNull.Value),
                new NpgsqlParameter("@MinPrice", minPrice ?? (object)DBNull.Value),
                new NpgsqlParameter("@MaxPrice", maxPrice ?? (object)DBNull.Value),
                new NpgsqlParameter("@HasDiscount", hasDiscount ?? (object)DBNull.Value),
                new NpgsqlParameter("@CategoryIds", categoryIds.Count > 0 ? (object)categoryIds.ToArray() : DBNull.Value),
                new NpgsqlParameter("@SubcategoryIds", subcategoryIds.Count > 0 ? (object)subcategoryIds.ToArray() : DBNull.Value),
                new NpgsqlParameter("@Offset", offset),
                new NpgsqlParameter("@Limit", limit)
            };
            return await _sessionIterator.ReadAsync(async context =>
            {
                var productList = new Dictionary<Guid, ProductInfoSearchDto>();
                int totalCount = 0;

                await using (var command = context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = commandText;
                    command.Parameters.AddRange(parameters);
                    if (command.Connection == null)
                    {
                        throw new InvalidOperationException("The database connection is null.");
                    }
                    if (command.Connection.State != System.Data.ConnectionState.Open)
                    {
                        await command.Connection.OpenAsync();
                    }

                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (totalCount == 0)
                            {
                                totalCount = reader.GetInt32(5);
                            }

                            var product = new ProductInfoSearchDto
                            {
                                Id = reader.GetGuid(0),
                                Name = reader.GetString(1),
                                Price = reader.GetDecimal(2),
                                CategoryId = reader.GetGuid(3),
                                Discount = reader.IsDBNull(4) ? (decimal?)null : reader.GetDecimal(4),
                                ImageUrls = reader.IsDBNull(6) ? new List<string>() : reader.GetFieldValue<List<string>>(6)
                            };
                            productList.Add(product.Id, product);
                        }
                    }
                }

                return (totalCount, productList);
            });
        }
        catch (Exception)
        {
            throw;
        }
    }



}

