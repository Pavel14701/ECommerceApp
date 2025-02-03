using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using System;
using System.IO;
using System.Threading.Tasks;

public class ProductUpdateService : IProductUpdateService
{
    private readonly string _connectionString;
    private readonly string _uploadPath;

    public ProductUpdateService(string connectionString)
    {
        _connectionString = connectionString;
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<ProductUpdateResultDto> UpdateProduct(Product product)
    {
        using var connection = new SqlConnection(_connectionString);
        var commandText = @"
            UPDATE Products
            SET Name = @Name, SubcategoryId = @SubcategoryId, Price = @Price, Stock = @Stock, Description = @Description
            WHERE Id = @Id";
        var command = new SqlCommand(commandText, connection);
        command.Parameters.AddWithValue("@Id", product.Id);
        command.Parameters.AddWithValue("@Name", product.Name);
        command.Parameters.AddWithValue("@SubcategoryId", product.SubcategoryId);
        command.Parameters.AddWithValue("@Price", product.Price);
        command.Parameters.AddWithValue("@Stock", product.Stock);
        command.Parameters.AddWithValue("@Description", product.Description);

        connection.Open();
        var rowsAffected = await command.ExecuteNonQueryAsync();

        return new ProductUpdateResultDto
        {
            ProductId = product.Id,
            Message = rowsAffected > 0 ? $"Product with ID: {product.Id} has been updated." : $"Product with ID: {product.Id} not found."
        };
    }

    public async Task<ProductUpdateResultDto> UpdateProductName(Guid id, string name)
    {
        using var connection = new SqlConnection(_connectionString);
        var commandText = "UPDATE Products SET Name = @Name WHERE Id = @Id";
        var command = new SqlCommand(commandText, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Name", name);

        connection.Open();
        var rowsAffected = await command.ExecuteNonQueryAsync();

        return new ProductUpdateResultDto
        {
            ProductId = id,
            Message = rowsAffected > 0 ? $"Product name updated to '{name}' for product with ID: {id}." : $"Product with ID: {id} not found."
        };
    }

    public async Task<ProductUpdateResultDto> UpdateProductCategory(Guid id, Guid categoryId)
    {
        using var connection = new SqlConnection(_connectionString);
        var commandText = "UPDATE Products SET CategoryId = @CategoryId WHERE Id = @Id";
        var command = new SqlCommand(commandText, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@CategoryId", categoryId);

        connection.Open();
        var rowsAffected = await command.ExecuteNonQueryAsync();

        return new ProductUpdateResultDto
        {
            ProductId = id,
            Message = rowsAffected > 0 ? $"Product category updated to '{categoryId}' for product with ID: {id}." : $"Product with ID: {id} not found."
        };
    }

    public async Task<ProductUpdateResultDto> UpdateProductSubcategory(Guid id, Guid subcategoryId)
    {
        using var connection = new SqlConnection(_connectionString);
        var commandText = "UPDATE Products SET SubcategoryId = @CategoryId WHERE Id = @Id";
        var command = new SqlCommand(commandText, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@CategoryId", subcategoryId);

        connection.Open();
        var rowsAffected = await command.ExecuteNonQueryAsync();

        return new ProductUpdateResultDto
        {
            ProductId = id,
            Message = rowsAffected > 0 ? $"Product category updated to '{subcategoryId}' for product with ID: {id}." : $"Product with ID: {id} not found."
        };
    }

    public async Task<ProductUpdateResultDto> UpdateProductPrice(Guid id, decimal price)
    {
        using var connection = new SqlConnection(_connectionString);
        var commandText = "UPDATE Products SET Price = @Price WHERE Id = @Id";
        var command = new SqlCommand(commandText, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Price", price);

        connection.Open();
        var rowsAffected = await command.ExecuteNonQueryAsync();

        return new ProductUpdateResultDto
        {
            ProductId = id,
            Message = rowsAffected > 0 ? $"Product price updated to '{price}' for product with ID: {id}." : $"Product with ID: {id} not found."
        };
    }

    public async Task<ProductUpdateResultDto> UpdateProductStock(Guid id, int stock)
    {
        using var connection = new SqlConnection(_connectionString);
        var commandText = "UPDATE Products SET Stock = @Stock WHERE Id = @Id";
        var command = new SqlCommand(commandText, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Stock", stock);

        connection.Open();
        var rowsAffected = await command.ExecuteNonQueryAsync();

        return new ProductUpdateResultDto
        {
            ProductId = id,
            Message = rowsAffected > 0 ? $"Product stock updated to '{stock}' for product with ID: {id}." : $"Product with ID: {id} not found."
        };
    }

    public async Task<ProductUpdateResultDto> UpdateProductDescription(Guid id, string description)
    {
        using var connection = new SqlConnection(_connectionString);
        var commandText = "UPDATE Products SET Description = @Description WHERE Id = @Id";
        var command = new SqlCommand(commandText, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Description", description);

        connection.Open();
        var rowsAffected = await command.ExecuteNonQueryAsync();

        return new ProductUpdateResultDto
        {
            ProductId = id,
            Message = rowsAffected > 0 ? $"Product description updated for product with ID: {id}." : $"Product with ID: {id} not found."
        };
    }

    public async Task<ImageUpdateResultDto> UpdateProductImage(Guid productId, Guid imageId, IFormFile file)
    {
        using var connection = new SqlConnection(_connectionString);
        
        // Проверяем наличие продукта и изображения
        var selectCommandText = @"
            SELECT p.Id AS ProductId, i.Id AS ImageId, i.ImageUrl
            FROM Products p
            JOIN Images i ON p.Id = i.ProductId
            WHERE p.Id = @ProductId AND i.Id = @ImageId";
        var selectCommand = new SqlCommand(selectCommandText, connection);
        selectCommand.Parameters.AddWithValue("@ProductId", productId);
        selectCommand.Parameters.AddWithValue("@ImageId", imageId);

        connection.Open();
        using var reader = await selectCommand.ExecuteReaderAsync();
        if (!reader.Read() || file == null || file.Length == 0)
        {
            return new ImageUpdateResultDto
            {
                Message = "Invalid product ID or file."
            };
        }

        var imageUrl = reader.GetString(reader.GetOrdinal("ImageUrl"));
        var filePath = Path.Combine(_uploadPath, imageUrl);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        reader.Close();

        // Удаляем старое изображение
        var deleteCommandText = "DELETE FROM Images WHERE Id = @ImageId";
        var deleteCommand = new SqlCommand(deleteCommandText, connection);
        deleteCommand.Parameters.AddWithValue("@ImageId", imageId);
        await deleteCommand.ExecuteNonQueryAsync();

        // Добавляем новое изображение
        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var newFilePath = Path.Combine(_uploadPath, fileName);

        using (var stream = new FileStream(newFilePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var insertCommandText = @"
            INSERT INTO Images (Id, ImageUrl, ProductId) 
            VALUES (@Id, @ImageUrl, @ProductId)";
        var insertCommand = new SqlCommand(insertCommandText, connection);
        var newImageId = Guid.NewGuid();
        insertCommand.Parameters.AddWithValue("@Id", newImageId);
        insertCommand.Parameters.AddWithValue("@ImageUrl", fileName);
        insertCommand.Parameters.AddWithValue("@ProductId", productId);

        await insertCommand.ExecuteNonQueryAsync();

        return new ImageUpdateResultDto
        {
            ImageId = newImageId,
            ImageUrl = fileName,
            Message = $"Image with ID: {imageId} has been updated for product with ID: {productId}."
        };
    }
}
