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

public class NewsCheckDto
{
    public Guid Id { get; set; }
    public string NewsTitle { get; set; } = string.Empty;
    public DateTime PublishDatetime { get; set; }
    public DateTime? UpdateDatetime { get; set; }
    public Guid? ImageId { get; set; }
    public string? ImageUrl { get; set; }
    public string? Alt { get; set; }
}


public class NewsPreviewDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public DateTime PublishDatetime { get; set; }
    public string? FirstBlockText { get; set; }
    public string? FirstImageUrl { get; set; }
}



public class NewsDataDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime PublishDatetime { get; set; }
    public int BlockNumber { get; set; }
    public string? Text { get; set; }
    public string[]? Images { get; set; }
}


public class NewsDetailsDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime PublishDatetime { get; set; }
    public Dictionary<int, object>? ContentBlocks { get; set; }
}

public class OrderDataDto
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public Guid? OrderItemId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public Guid? OrderDiscountId { get; set; }
    public string? DiscountCode { get; set; }
    public decimal? DiscountAmount { get; set; }
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
    public string Description { get; set; } = string.Empty;
    public int Stock { get; set; }
    public int? Discount { get; set; }
    public int TotalCount { get; set; }
    public List<string> ImageUrls { get; set; } = new List<string>();
}





public interface IReadCrud
{
    Task<bool?> CheckNews(CheckNewsCrudDto news);
    Task<Guid> GetNewsIdByTitleAndDate(ReadNewsIdDto paramsDto);
    Task<List<NewsPreviewDto>> GetPaginatedNews(PaginationParamsDto paramsDto);
    Task<NewsDetailsDto?> GetNewsDetailsById(Guid newsId);
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

public async Task<bool?> CheckNews(CheckNewsCrudDto news)
{
    try
    {
        var sql = @"
            SELECT n.id, n.news_title, n.publish_datetime, n.update_datetime, ni.id AS ImageId, ni.image_url, ni.alt 
            FROM news n
            LEFT JOIN news_images_relationship nir ON n.id = nir.fk_news_id
            LEFT JOIN images ni ON nir.image_id = ni.id
            WHERE n.id = @NewsId;
        ";
        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter("@NewsId", news.NewsId)
        };
        return await _sessionIterator.ReadAsync<bool?>(async context =>
        {
            var result = await context.Set<NewsCheckDto>().FromSqlRaw(sql, parameters).FirstOrDefaultAsync();
            return (result is not null) ? true : null;
        });
    }
    catch (Exception ex)
    {
        throw new Exception("An error occurred while checking the news.", ex);
    }
}



public async Task<Guid> GetNewsIdByTitleAndDate(ReadNewsIdDto paramsDto)
{
    try
    {
        var sql = @"
            SELECT n.id
            FROM news n
            WHERE n.news_title = @Title AND n.publish_datetime = @PublishDatetime;
        ";
        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter("@Title", paramsDto.Title),
            new NpgsqlParameter("@PublishDatetime", paramsDto.PublishDatetime)
        };
        var result = await _sessionIterator.ReadAsync<Guid>(async context =>
        {
            var newsId = await context.Set<CheckNewsCrudDto>().FromSqlRaw(sql, parameters).FirstOrDefaultAsync();
            return newsId?.NewsId ?? Guid.Empty;
        });
        return result;
    }
    catch (Exception ex)
    {
        throw new Exception("An error occurred while fetching the news ID.", ex);
    }
}


public async Task<List<NewsPreviewDto>> GetPaginatedNews(PaginationParamsDto paramsDto)
{
    try
    {
        var offset = (paramsDto.Page - 1) * paramsDto.PageSize;
        var sql = @"
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
        var parameters = new[]
        {
            new NpgsqlParameter("@Offset", offset),
            new NpgsqlParameter("@PageSize", paramsDto.PageSize)
        };
        return await _sessionIterator.ReadAsync(async context =>
        {
            var newsList = await context.Set<NewsPreviewDto>().FromSqlRaw(sql, parameters).ToListAsync();
            return newsList;
        });
    }
    catch (Exception)
    {
        throw;
    }
}

    public async Task<NewsDetailsDto?> GetNewsDetailsById(Guid newsId)
    {
        try
        {
            var sql = @"
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
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@NewsId", newsId)
            };
            return await _sessionIterator.ReadAsync(async context =>
            {
                var newsData = await context.Set<NewsDataDto>().FromSqlRaw(sql, parameters).ToListAsync();
                if (newsData == null || newsData.Count == 0)
                {
                    return null;
                }
                var newsDetails = new NewsDetailsDto
                {
                    Id = newsData[0].Id,
                    Title = newsData[0].Title,
                    PublishDatetime = newsData[0].PublishDatetime,
                    ContentBlocks = new Dictionary<int, object>()
                };
                foreach (var data in newsData)
                {
                    var blockNumber = data.BlockNumber;
                    if (data.Text != null)
                    {
                        newsDetails.ContentBlocks[blockNumber] = data.Text;
                    }
                    else if (data.Images != null && data.Images.Length > 0)
                    {
                        newsDetails.ContentBlocks[blockNumber] = data.Images
                            .Select(imageJson => JsonConvert.DeserializeObject<Dictionary<string, string>>(imageJson))
                            .ToList();
                    }
                }
                return newsDetails;
            });
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
            var sql = @"
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
            return await _sessionIterator.ReadAsync(async context =>
            {
                var orderData = await context.Set<OrderDataDto>().FromSqlRaw(sql, parameters).ToListAsync();
                foreach (var data in orderData)
                {
                    if (!orderDictionary.TryGetValue(data.OrderId, out var order))
                    {
                        order = new OrderDto
                        {
                            Id = data.OrderId,
                            UserId = data.UserId,
                            OrderDate = data.OrderDate,
                            TotalAmount = data.TotalAmount,
                            OrderItems = new List<OrderItemDto>(),
                            OrderDiscounts = new List<OrderDiscountDto>()
                        };
                        orderDictionary[data.OrderId] = order;
                    }
                    if (data.OrderItemId.HasValue)
                    {
                        var orderItem = new OrderItemDto
                        {
                            Id = data.OrderItemId.Value,
                            Quantity = data.Quantity,
                            UnitPrice = data.UnitPrice
                        };
                        order.OrderItems.Add(orderItem);
                    }
                    if (data.OrderDiscountId.HasValue)
                    {
                        var orderDiscount = new OrderDiscountDto
                        {
                            Id = data.OrderDiscountId.Value,
                            Code = data.DiscountCode ?? string.Empty,
                            Amount = data.DiscountAmount ?? 0m
                        };
                        order.OrderDiscounts.Add(orderDiscount);
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


    public async Task<IEnumerable<ProductPreviewDto>> SearchProducts(
        string? name, decimal? minPrice, decimal? maxPrice, bool? hasDiscount, Dictionary<Guid, List<Guid>>? categorySubcategoryDict, int offset, int limit)
    {
        try
        {
            var categoryIds = categorySubcategoryDict?.Keys.ToList() ?? new List<Guid>();
            var subcategoryIds = categorySubcategoryDict?.Values.SelectMany(v => v).ToList() ?? new List<Guid>();

            var sql = @"
                WITH ProductData AS (
                    SELECT 
                        p.id AS ProductId,
                        p.name AS ProductName,
                        p.price AS Price,
                        p.discount AS Discount,
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
                        (@HasDiscount IS NULL OR (@HasDiscount = TRUE AND p.discount IS NOT NULL) OR (@HasDiscount = FALSE AND p.discount IS NULL)) AND
                        (cr.fk_category = ANY(@CategoryIds) OR @CategoryIds IS NULL) AND
                        (cr.fk_subcategory = ANY(@SubcategoryIds) OR @SubcategoryIds IS NULL)
                ),
                ImageData AS (
                    SELECT
                        pir.fk_product_id AS ProductId,
                        pi.image_url AS ImageUrl,
                        ROW_NUMBER() OVER (PARTITION BY pir.fk_product_id ORDER BY pi.image_url) AS ImageRowNum
                    FROM 
                        product_image_relationship pir
                    LEFT JOIN
                        images pi ON pir.image_id = pi.id
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
            var parameters = new List<NpgsqlParameter>
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
                var products = await context.Set<ProductPreviewDto>().FromSqlRaw(sql, parameters).ToListAsync();
                return products;
            }
        );
        }
        catch (Exception)
        {
            throw;
        }
    }


    public async Task<(int TotalCount, Dictionary<Guid, ProductInfoSearchDto> Products)> SearchProductsInfo(
        string? name, decimal? minPrice, decimal? maxPrice, bool? hasDiscount, Dictionary<Guid, List<Guid>>? categorySubcategoryDict, int offset, int limit)
    {
        try
        {
            var categoryIds = categorySubcategoryDict?.Keys.ToList() ?? new List<Guid>();
            var subcategoryIds = categorySubcategoryDict?.Values.SelectMany(v => v).ToList() ?? new List<Guid>();
            var sql = @"
                WITH ProductData AS (
                    SELECT 
                        p.id AS ProductId,
                        p.name AS ProductName,
                        p.price AS Price,
                        p.description AS Description,
                        p.discount AS Discount,
                        p.stock AS Stock,
                        ROW_NUMBER() OVER (ORDER BY p.name) AS RowNum,
                        COUNT(*) OVER() AS TotalCount
                    FROM 
                        products p
                    LEFT JOIN 
                        category_relationship cr ON p.id = cr.fk_product
                    WHERE 
                        (@Name IS NULL OR p.name ILIKE '%' || @Name || '%') AND
                        (@MinPrice IS NULL OR p.price >= @MinPrice) AND
                        (@MaxPrice IS NULL OR p.price <= @MaxPrice) AND
                        (@HasDiscount IS NULL OR (@HasDiscount = TRUE AND p.discount IS NOT NULL) OR (@HasDiscount = FALSE AND p.discount IS NULL)) AND
                        (cr.fk_category = ANY(@CategoryIds::uuid[]) OR @CategoryIds IS NULL) AND
                        (cr.fk_subcategory = ANY(@SubcategoryIds::uuid[]) OR @SubcategoryIds IS NULL)
                ),
                ImageData AS (
                    SELECT
                        pir.fk_product_id AS ProductId,
                        pi.image_url AS ImageUrl,
                        ROW_NUMBER() OVER (PARTITION BY pir.fk_product_id ORDER BY pi.image_url) AS ImageRowNum
                    FROM 
                        product_image_relationship pir
                    LEFT JOIN
                        images pi ON pir.image_id = pi.id
                )
                SELECT 
                    pd.ProductId,
                    pd.ProductName,
                    pd.Price,
                    pd.Description,
                    pd.Discount,
                    pd.Stock,
                    pd.TotalCount,
                    array_agg(i.ImageUrl) FILTER (WHERE i.ImageRowNum <= 5) AS ImageUrls
                FROM 
                    ProductData pd
                LEFT JOIN 
                    ImageData i ON pd.ProductId = i.ProductId
                WHERE 
                    pd.RowNum BETWEEN @Offset AND @Offset + @Limit - 1
                GROUP BY 
                    pd.ProductId, pd.ProductName, pd.Price, pd.Description, pd.Discount, pd.Stock, pd.TotalCount
                ORDER BY 
                    pd.RowNum;
            ";
            var parameters = new List<NpgsqlParameter>
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
            var productList = new Dictionary<Guid, ProductInfoSearchDto>();
            int totalCount = 0;
            return await _sessionIterator.ReadAsync(async context =>
                {
                    var products = await context.Set<ProductInfoSearchDto>().FromSqlRaw(sql, parameters).ToListAsync();
                    foreach (var product in products)
                    {
                        if (totalCount == 0)
                        {
                            totalCount = product.TotalCount;
                        }
                        productList.Add(product.Id, product);
                    }
                return (totalCount, productList);
                }
            );
        }
        catch (Exception )
        {
            throw;
        }
    }



}

