using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public interface IProductService
{
    Task<IEnumerable<Product>> GetAllProducts(int pageNumber, int pageSize);
    Task<IEnumerable<Product>> GetProductsByCategory(string category, int pageNumber, int pageSize);
    Task<IEnumerable<Product>> GetProductsByName(string name, int pageNumber, int pageSize);
    Task<Product?> GetProductById(Guid id);
    Task AddProduct(Product product);
    Task UpdateProduct(Product product);
    Task DeleteProduct(Guid id);
    Task AddImageToProduct(Guid productId, Images image);
    Task RemoveImageFromProduct(Guid productId, Guid imageId);
    Task UpdateProductName(Guid id, string name);
    Task UpdateProductCategory(Guid id, string category);
    Task UpdateProductPrice(Guid id, decimal price);
    Task UpdateProductStock(Guid id, int stock);
    Task UpdateProductDescription(Guid id, string description);
    Task<Images?> UploadImage(Guid productId, IFormFile file);
    Task<bool> DeleteImage(Guid productId, Guid imageId);
    Task<Images?> UpdateImage(Guid productId, Guid imageId, IFormFile file);
}
