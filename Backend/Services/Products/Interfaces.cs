



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
