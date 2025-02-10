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
    public required Dictionary<string, IFormFile> Images { get; set; } 
}


public class ProductCreateService : IProductCreateService
{
    private readonly CreateCrud _createCrud;
    private readonly ImageUploader _imageUploader;

    public ProductCreateService
    (
        CreateCrud createCrud,
        ImageUploader imageUploader
    )
    {
        _createCrud = createCrud;
        _imageUploader = imageUploader;
    }

    public async Task<Result> AddProduct(CreateProductParamsDto paramsDto)
    {
        var newImagesDict = new Dictionary<Guid, string>();
        foreach (var kvp in paramsDto.Images)
        {
            var newImageId = Guid.NewGuid();
            var result = await _imageUploader.UploadImage(new ImageUploadParamsDto{
                ImageId = newImageId,
                File = kvp.Value
            });
            newImagesDict.Add(newImageId, result.FilePath?? throw new Exception("Path is nulled"));
        }
        try
        {
            var productId = Guid.NewGuid();
            var relationshipId = Guid.NewGuid();
            await _createCrud.CreateProduct(new CreateProductDto
            {
                Id = productId,
                RelationshipId = relationshipId,
                Name = paramsDto.Name,
                CategoryId = paramsDto.CategoryId,
                SubcategoryId = paramsDto.SubcategoryId,
                Price = paramsDto.Price,
                Stock = paramsDto.Stock,
                Description = paramsDto.Description
            });
            var imagesDataList = new List<CreateImageDto>();
            using (var enum1 = newImagesDict.GetEnumerator())
            using (var enum2 = paramsDto.Images.GetEnumerator())
            {
                while (enum1.MoveNext() && enum2.MoveNext())
                {
                    var result = enum1.Current;
                    var param = enum2.Current;
                    imagesDataList.Add(new CreateImageDto
                    {
                        Id = result.Key,
                        ImageUrl = result.Value,
                        Alt = param.Key
                    });
                }
            }
            await _createCrud.AddImagesToProduct(new AddProductImage
            {
                ProductId = productId,
                Images = imagesDataList
            });
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