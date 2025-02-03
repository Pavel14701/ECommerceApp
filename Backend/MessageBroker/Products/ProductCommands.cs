public class CreateProductCommand
{
    public Guid CommandId { get; set; }
    public required Product Product { get; set; }
}

public class DeleteProductCommand
{
    public Guid CommandId { get; set; }
    public Guid ProductId { get; set; }
}

public class DeleteImageCommand
{
    public Guid CommandId { get; set; }
    public Guid ObjectId { get; set; }
    public Guid ImageId { get; set; }
}

public class GetAllProductsQuery
{
    public Guid QueryId { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class GetProductsByCategoryQuery
{
    public Guid QueryId { get; set; }
    public required string Category { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class GetProductsByNameQuery
{
    public Guid QueryId { get; set; }
    public required string Name { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class GetProductByIdQuery
{
    public Guid QueryId { get; set; }
    public Guid ProductId { get; set; }
}

public class UpdateProductNameCommand
{
    public Guid CommandId { get; set; }
    public Guid ProductId { get; set; }
    public required string Name { get; set; }
}

public class UpdateProductCategoryCommand
{
    public Guid CommandId { get; set; }
    public Guid ProductId { get; set; }
    public required Guid CategoryId { get; set; }
}


public class UpdateProductSubcategoryCommand
{
    public Guid CommandId { get; set; }
    public Guid ProductId { get; set; }
    public required Guid SubcategoryId { get; set; }
}

public class UpdateProductPriceCommand
{
    public Guid CommandId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Price { get; set; }
}

public class UpdateProductStockCommand
{
    public Guid CommandId { get; set; }
    public Guid ProductId { get; set; }
    public int Stock { get; set; }
}

public class UpdateProductDescriptionCommand
{
    public Guid CommandId { get; set; }
    public Guid ProductId { get; set; }
    public required string Description { get; set; }
}

public class UpdateProductImageCommand
{
    public Guid CommandId { get; set; }
    public Guid ProductId { get; set; }
    public Guid ImageId { get; set; }
    public required IFormFile File { get; set; }
}

public class UpdateProductCommand 
{
    public Guid CommandId { get; set; }
    public required Product Product { get; set; } 
}