public interface IProductCreateService
{
    Task<ProductCreationResultDto> AddProduct(Product product);
    Task<ImageUploadResultDto> UploadImage(Guid productId, IFormFile file);
}

public interface IProductDeleteService
{
    Task<ProductDeletionResultDto> DeleteProduct(Guid id);
    Task<ImageDeletionResultDto> DeleteImage(Guid productId, Guid imageId);
}

public interface IProductReadService
{
    Task<PagedProductsDto> GetAllProducts(int pageNumber, int pageSize);
    Task<PagedProductsDto> GetProductsByCategory(string category, int pageNumber, int pageSize);
    Task<PagedProductsDto> GetProductsByName(string name, int pageNumber, int pageSize);
    Task<ProductDto> GetProductById(Guid id);
}

public interface IProductUpdateService
{
    Task<ProductUpdateResultDto> UpdateProduct(Product product);
    Task<ProductUpdateResultDto> UpdateProductName(Guid id, string name);
    Task<ProductUpdateResultDto> UpdateProductCategory(Guid id, Guid categoryId);
    Task<ProductUpdateResultDto> UpdateProductSubcategory(Guid id, Guid subcategoryId);
    Task<ProductUpdateResultDto> UpdateProductPrice(Guid id, decimal price);
    Task<ProductUpdateResultDto> UpdateProductStock(Guid id, int stock);
    Task<ProductUpdateResultDto> UpdateProductDescription(Guid id, string description);
    Task<ImageUpdateResultDto> UpdateProductImage(Guid productId, Guid imageId, IFormFile file);
}
