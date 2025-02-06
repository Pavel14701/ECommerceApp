public class DeleteNewsParamsDto : ReadNewsIdDto
{}

public class RemoveNewsImageParamsDto : ReadNewsIdDto
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
    private readonly SessionIterator _sessionIterator;
    private readonly ReadCrud _readCrud;
    private readonly DeleteCrud _delCrud;
    private readonly ImageUploader _imageUploader;
    public DeleteNewsService(
        SessionIterator sessionIterator,
        ReadCrud readCrud,
        DeleteCrud delCrud,
        ImageUploader imageUploader
    )
    {
        _sessionIterator = sessionIterator;
        _readCrud = readCrud;
        _delCrud = delCrud;
        _imageUploader = imageUploader;
    }

    public async Task<Result> DeleteNews(DeleteNewsParamsDto paramsDto)
    {
        var result = await _readCrud.GetNewsIdByTitleAndDate(
            new ReadNewsIdDto
            {
                Title = paramsDto.Title,
                PublishDatetime = paramsDto.PublishDatetime
            }
        );
        if (result is not null)
        {
            try
            {
                await _sessionIterator.ExecuteAsync(async context =>
                {
                    await _delCrud.DeleteNews(
                        new DeleteNewsParamsCrudDto
                        {
                            Context = context,
                            Id = result.Id
                        }
                    );
                    var imgIds = await _delCrud.DeleteAllImagesByNewsId(
                        new DeleteImageParamsCrudDto
                        {
                            Context = context,
                            Id = result.Id
                        }
                    );
                    await _delCrud.DeleteAllTextBlocksByNewsId(
                        new DeleteTextParamsCrudDto
                        {
                            Context = context,
                            Id = result.Id
                        }
                    );
                    await _imageUploader.DeleteImages(
                        new ImageDeleteParamsDto
                        {
                            ImageIds = imgIds
                        }
                    );
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
        return new Result
        {
            Success = false,
            Message = "News not founded"
        };
    }

    public async Task<Result> RemoveImageFromNews(RemoveNewsImageParamsDto paramsDto)
    {
        var result = await _readCrud.GetNewsIdByTitleAndDate(
            new ReadNewsIdDto
            {
                Title = paramsDto.Title,
                PublishDatetime = paramsDto.PublishDatetime
            }
        );
        if ( result is not null )
        {
            try
            {
                await _sessionIterator.ExecuteAsync(async context =>
                {
                    var imgIds = await _delCrud.DeleteImagesByNewsIdAndBlockNumber(
                        new DeleteNewsImageParamsDto
                        {
                            Context = context, 
                            Id = result.Id,
                            BlockNumber = paramsDto.BlockNumber
                        }
                    );
                    await _imageUploader.DeleteImages(
                        new ImageDeleteParamsDto
                        {
                            ImageIds = imgIds
                        }
                    );
                }
            );
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
        return new Result
        {
            Success = false,
            Message = "News not founded"
        };
    } 


    public async Task<Result> RemoveTextBlockFromNews(RemoveTextBlockParamsDto paramsDto)
    {
        var result = await _readCrud.GetNewsIdByTitleAndDate(
            new ReadNewsIdDto
            {
                Title = paramsDto.Title,
                PublishDatetime = paramsDto.PublishDatetime
            }
        );
        if ( result is not null )
        {
            try
            {
                await _sessionIterator.ExecuteAsync(async context =>
                {
                    await _delCrud.DeleteTextBlockByNewsIdAndBlockNumber(
                        new DeleteTextBlockImageParamsDto
                        {
                            Context = context,
                            Id = result.Id,
                            BlockNumber = paramsDto.BlockNumber
                        }
                    );
                }
                );
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
        return new Result
        {
            Success = false,
            Message = "News not founded"
        };
    }

}
