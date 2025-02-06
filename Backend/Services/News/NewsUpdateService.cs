public interface IUpdateNewsService
{
    Task<Result> UpdateNewsPreview();
    Task<Result> UpdateNewsTitle();
    Task<Result> UpdateNewsPreviewImage();
    Task<Result> UpdateNewsImageBlock();
    Task<Result> UpdateNewsContentBlock();
}


public class UpdateNewsService : IUpdateNewsService
{
    private readonly SessionIterator _sessionIterator;
    private readonly ImageUploader _imageUploader;
    private readonly UpdateCrud _updateCrud;
    private readonly ReadCrud _readCrud;

    public UpdateNewsService(
        SessionIterator sessionIterator,
        ImageUploader imageUploader,
        UpdateCrud updateCrud,
        ReadCrud readCrud
    )
    {
        _sessionIterator = sessionIterator;
        _imageUploader = imageUploader;
        _updateCrud = updateCrud;
        _readCrud = readCrud;
    }

    public async Task<Result> UpdateNewsPreview()
    {
        try{
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

    public async Task<Result> UpdateNewsTitle()
    {
        try{
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

    public async Task<Result> UpdateNewsPreviewImage()
    {
        try{
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

    public async Task<Result> UpdateNewsImageBlock()
    {
        try{
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

    public async Task<Result> UpdateNewsContentBlock()
    {
        try{
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
