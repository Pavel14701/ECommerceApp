public class AddNewsParamsDto
{
    public required string TextTitle { get; set; }
    public required IFormFile File { get; set; }
    public required string AltText { get; set; }
    public required string TextContent { get; set; }
}

public class AddImageNewsDto
{
    public required Guid NewsId { get; set; }
    public required List<IFormFile> Images { get; set; }
    public required List<string> AltTexts { get; set; }
    public required int BlockNumber { get; set; }
}

public class AddTextNewsDto
{
    public required string Title { get; set; }
    public required DateTime PublishDatetime { get; set; }
    public required int BlockNumber { get; set; }
    public required string TextContent { get; set; }
    public required Guid NewsId { get; set; }
}



public interface ICreateNewsService
{
    Task<Result> AddNews(AddNewsParamsDto paramsDto);
    Task<Result> AddImageToNews(AddImageNewsDto paramsDto);
    Task<Result> AddTextToNews(AddTextNewsDto paramsDto);
}




public class CreateNewsService : ICreateNewsService
{
    private readonly SessionIterator _sessionIterator;
    private readonly CreateCrud _createCrud;
    private readonly ReadCrud _readCrud;
    private readonly ImageUploader _imageUploader;
    public CreateNewsService(
        CreateCrud createCrud,
        ReadCrud readCrud,
        SessionIterator sessionIterator,
        ImageUploader imageUploader
    )
    {
        _sessionIterator = sessionIterator;
        _createCrud = createCrud;
        _readCrud = readCrud;
        _imageUploader = imageUploader;
    }

    public async Task<Result> AddNews(AddNewsParamsDto paramsDto)
    {
        var imageId = Guid.NewGuid();
        var publishDatetime = DateTime.UtcNow;
        var updateDatetime = DateTime.UtcNow;
        var newsId = Guid.NewGuid();
        var newsContentIdImage = Guid.NewGuid();
        var newsContentIdText = Guid.NewGuid();
        try
        {
            await _sessionIterator.ExecuteAsync(async context =>
            {
                await _createCrud.AddNews(
                    new CreateNewsCrudDto
                    {
                        Context = context, 
                        Id = newsId,
                        Title = paramsDto.TextTitle,
                        PublishDatetime = publishDatetime,
                        UpdateDatetime = updateDatetime
                    }
                );
                var result = await _imageUploader.UploadImage(
                    new ImageUploadParamsDto
                    {
                        ImageId = imageId,
                        File = paramsDto.File,
                    }
                );
                if (result.FilePath == null)
                {
                    throw new Exception("File upload failed, no file path returned.");
                }
                await _createCrud.AddImage(
                    new CreateImageCrudDto
                    {
                        Context = context,
                        Id = imageId,
                        ImageUrl = result.FilePath,
                        NewsId = newsId,
                        AltText = paramsDto.AltText
                    }
                );
                await _createCrud.AddNewsContentImage(
                    new CreateContentImageCrudDto
                    {
                        Context = context,
                        Id = Guid.NewGuid(),
                        ImageId = imageId,
                        BlockNumber = 1
                    }
                );
                await _createCrud.AddNewsContentText(
                    new CreateContentTextCrudDto
                    {
                        Context = context,
                        Id = Guid.NewGuid(),
                        TextContent = paramsDto.TextContent,
                        BlockNumber = 2
                    }
                );
            });
            return new Result
            {
                Success = true,
                Message = $"News with ID: {newsId} has been created."
            };
        }
        catch (Exception ex)
        {
            List<Guid> imageIds = new List<Guid> { imageId };
            var result = await _imageUploader.DeleteImages(
                new ImageDeleteParamsDto
                {
                    ImageIds = imageIds
                }
            );
            return new Result
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }


    public async Task<Result> AddImageToNews(AddImageNewsDto paramsDto)
    {
        var uploadResults = new Dictionary<Guid, ImageUplodedDto>();
        foreach (var file in paramsDto.Images)
        {
            var imageId = Guid.NewGuid();
            uploadResults[imageId] = new ImageUplodedDto();
        }        
        var checkResult = await _readCrud.CheckNews(
            new CheckNewsCrudDto
            {
                NewsId = paramsDto.NewsId
            }
        );
        if (checkResult.Success)
        {
            try
            {
                await _sessionIterator.ExecuteAsync(async context =>
                {
                    foreach (var kvp in uploadResults.ToList())
                    {
                        var imageId = kvp.Key;
                        var file = paramsDto.Images[uploadResults.Keys.ToList().IndexOf(imageId)];
                        var uploadParams = new ImageUploadParamsDto
                        {
                            File = file,
                            ImageId = imageId
                        };
                        var result = await _imageUploader.UploadImage(uploadParams);
                        uploadResults[imageId] = result;
                    }
                    if (uploadResults.Count == paramsDto.AltTexts.Count)
                    {
                        int index = 0;
                        foreach (var kvp in uploadResults)
                        {
                            await _createCrud.AddImage(
                                new CreateImageCrudDto
                                {
                                    Context = context,
                                    Id = kvp.Key,
                                    ImageUrl = kvp.Value.FilePath ?? string.Empty, // Проверяем на null
                                    AltText = paramsDto.AltTexts[index],
                                    NewsId = paramsDto.NewsId
                                }
                            );
                            await _createCrud.AddNewsContentImage(
                                new CreateContentImageCrudDto
                                {
                                    Context = context,
                                    ImageId = kvp.Key,
                                    Id = Guid.NewGuid(),
                                    BlockNumber = paramsDto.BlockNumber
                                }
                            );
                            index++;
                        }
                    }
                });
                return new Result
                {
                    Success = true,
                    Message = @$"Images have been added to News with ID: {paramsDto.NewsId}."
                };
            }
            catch (Exception ex)
            {
                await _imageUploader.DeleteImages(
                    new ImageDeleteParamsDto
                    {
                        ImageIds = uploadResults.Keys.ToList()
                    }
                );
                return new Result
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }
        return new Result
        {
            Success = false,
            Message = $"News with ID: {paramsDto.NewsId} not found."
        };
    }

    public async Task<Result> AddTextToNews(AddTextNewsDto paramsDto)
    {
        var result = await _readCrud.GetNewsIdByTitleAndDate(
            new ReadNewsIdDto
            {
                Title = paramsDto.Title,
                PublishDatetime = paramsDto.PublishDatetime
            }
        );
        if (result != null)
        {
            await _sessionIterator.ExecuteAsync(async context =>
            {
                await _createCrud.AddNewsContentText(
                    new CreateContentTextCrudDto
                    {
                        Context = context,
                        TextContent = paramsDto.TextContent,
                        Id = Guid.NewGuid(),
                        NewsId = result.Id,
                        BlockNumber = paramsDto.BlockNumber
                    }
                );
            });
            return new Result
            {
                Success = true,
                Message = "Image Added"
            };
        }
        return new Result
        {
            Success = false,
            Message = @$"
                News with title: {paramsDto.Title} and
                publish time {paramsDto.PublishDatetime} is not found"
        };

    }
}