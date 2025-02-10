public interface IProductCreateService
{
    Task<Result> AddProduct(CreateProductParamsDto paramsDto);
}

public class CreateProductParamsDto
{
    public string Name { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Guid SubcategoryId { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Description { get; set; } = string.Empty;
}


public class ProductCreateService : IProductCreateService
{
    private readonly CreateCrud _createCrud;
    private readonly ImageUploader _imageUploader;

    public ProductCreateService
    (
        SessionIterator sessionIterator,
        CreateCrud createCrud,
        ImageUploader imageUploader
    )
    {
        _sessionIterator = sessionIterator;
        _createCrud = createCrud;
        _imageUploader = imageUploader;
    }

    public async Task<Result> AddProduct(CreateProductParamsDto paramsDto)
    {
        try
        {
            var productId = Guid.NewGuid();
            var relationshipId = Guid.NewGuid();
            return new Result
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new Result
            {
                Success = false,
                Message = $"Error: {ex}"
            };
        }
    }
}
