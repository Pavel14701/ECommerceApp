public interface IProductDeleteService
{
    Task<Result> DeleteProduct(Guid id);
    Task<Result> DeleteImage(Guid productId, Guid imageId);
}


public class ProductDeleteService : IProductDeleteService
{
    private readonly SessionIterator _sessionIterator;
    private readonly DeleteCrud _delCrud;
    private readonly ImageUploader _imageUploader;

    public ProductDeleteService
    (
        SessionIterator sessionIterator,
        DeleteCrud delCrud,
        ImageUploader imageUploader
    )
    {
        _sessionIterator = sessionIterator;
        _delCrud = delCrud;
        _imageUploader = imageUploader;
    }

    public async Task<Result> DeleteProduct(Guid id)
    {
        try
        {
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

    public async Task<Result> DeleteImage(Guid productId, Guid imageId)
    {
        try
        {
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
