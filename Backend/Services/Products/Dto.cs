public class ProductCreationResultDto
{
    public Guid ProductId { get; set; }
    public required string Message { get; set; }
}

public class ProductDeletionResultDto
{
    public Guid ProductId { get; set; }
    public required string Message { get; set; }
}

public class ImageDeletionResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class PagedProductsDto
{
    public int TotalCount { get; set; }
    public IEnumerable<ProductDto> Products { get; set; } = new List<ProductDto>();
}

public class ProductDto
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public string? Category { get; set; }
    public decimal? Price { get; set; }
    public int? Stock { get; set; }
    public string? Description { get; set; }
    public IEnumerable<ImageDto>? Images { get; set; } = new List<ImageDto>();
}

public class ImageDto
{
    public Guid? Id { get; set; }
    public string? ImageUrl { get; set; }
    public string? Alt { get; set; }
}

public class ProductUpdateResultDto
{
    public Guid ProductId { get; set; }
    public required string Message { get; set; }
}

public class ImageUpdateResultDto
{
    public Guid? ImageId { get; set; }
    public string?  ImageUrl { get; set; } = string.Empty;
    public required string Message { get; set; } = string.Empty;
}