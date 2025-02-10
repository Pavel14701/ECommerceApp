public class DeleteNewsParamsDto : ReadNewsIdDto
{
    public required Guid NewsId { get; set;}
}

public class RemoveNewsImageParamsDto : DeleteNewsParamsDto
{
    public required int BlockNumber { get; set; }
}

public class RemoveTextBlockParamsDto : RemoveNewsImageParamsDto
{}


public interface IDeleteNewsService
{
    Task<Result> DeleteNews(DeleteNewsParamsDto paramsDto);
    Task<Result> RemoveImageFromNews(RemoveNewsImageParamsDto paramsDto);
    Task<Result> RemoveTextBlockFromNews(RemoveTextBlockParamsDto paramsDto);
}




public class DeleteNewsService : IDeleteNewsService
{
    private readonly DeleteCrud _delCrud;
    private readonly ImageUploader _imageUploader;
    public DeleteNewsService(
        DeleteCrud delCrud,
        ImageUploader imageUploader
    )
    {
        _delCrud = delCrud;
        _imageUploader = imageUploader;
    }

    public async Task<Result> DeleteNews(DeleteNewsParamsDto paramsDto)
    {
        try{
            await _delCrud.DeleteNews
            (
                new DeleteNewsParamsCrudDto{ Id = paramsDto.NewsId }
            );
            var imgIds = await _delCrud.DeleteAllImagesByNewsId
            (
                new DeleteImageParamsCrudDto{ Id = paramsDto.NewsId }
            );
            await _delCrud.DeleteAllTextBlocksByNewsId
            (
                new DeleteTextParamsCrudDto{ Id = paramsDto.NewsId }
            );
            await _imageUploader.DeleteImages
            (
                new ImageDeleteParamsDto{ ImageIds = imgIds }
            );
            return new Result{ Success = true };
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

    public async Task<Result> RemoveImageFromNews(RemoveNewsImageParamsDto paramsDto)
    {
        try{
            var imgIds = await _delCrud.DeleteImagesByNewsIdAndBlockNumber(
                new DeleteNewsImageParamsDto
                {
                    Id = paramsDto.NewsId,
                    BlockNumber = paramsDto.BlockNumber
                }
            );
            await _imageUploader.DeleteImages
            (
                new ImageDeleteParamsDto{ ImageIds = imgIds }
            );
            return new Result{ Success = true };
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


    public async Task<Result> RemoveTextBlockFromNews(RemoveTextBlockParamsDto paramsDto)
    {
        try{
            await _delCrud.DeleteTextBlockByNewsIdAndBlockNumber(
                new DeleteTextBlockImageParamsDto
                {
                    Id = paramsDto.NewsId,
                    BlockNumber = paramsDto.BlockNumber
                }
            );
            return new Result{ Success = true };
        }
        catch (Exception ex)
        {
            return new Result{ Success = false, Message = $"Error: {ex}" };
        }
    }
}
