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
    private readonly CreateCrud _createCrud;
    private readonly ReadCrud _readCrud;
    private readonly ImageUploader _imageUploader;
    public CreateNewsService(
        CreateCrud createCrud,
        ReadCrud readCrud,
        ImageUploader imageUploader
    )
    {
        _createCrud = createCrud;
        _readCrud = readCrud;
        _imageUploader = imageUploader;
    }

    public async Task<Result> AddNews(AddNewsParamsDto paramsDto)
    {
        var imageId = Guid.NewGuid();
        try {
        var result = await _imageUploader.UploadImage(new ImageUploadParamsDto{
            ImageId = imageId,
            File = paramsDto.File
        });
        await _createCrud.AddNews(new CreateNewsCrudDto{
            Id = Guid.NewGuid(),
            AltText = paramsDto.AltText,
            ImageUrl = result.FilePath?? throw new Exception(),
            ImageId = imageId,
            ContentIdImage = Guid.NewGuid(),
            ContentIdText = Guid.NewGuid(),
            NewsTitle = paramsDto.TextTitle,
            TextContent = paramsDto.TextContent
        });
        return new Result
        {
            Success = true,
            Message = $"News has been created."
        };}
        catch (Exception ex)
        {
            try
            {
                List<Guid> imageIds = new List<Guid> { imageId };
                var result = await _imageUploader.DeleteImages(new ImageDeleteParamsDto{ImageIds = imageIds});
            }catch{}
            return new Result{
                Success = false, Message = $"Error: {ex.Message}"
            };
        }
    }


    public async Task<Result> AddImageToNews(AddImageNewsDto paramsDto)
    {
        var checkResult = await _readCrud.CheckNews(new CheckNewsCrudDto { NewsId = paramsDto.NewsId });
        if (checkResult is true)
        {
            Dictionary<Guid, ImageUplodedDto> uploadResults = new Dictionary<Guid, ImageUplodedDto>();
            try
            {
                foreach (var file in paramsDto.Images)
                {
                    var imageId = Guid.NewGuid();
                    uploadResults[imageId] = new ImageUplodedDto();
                }
                foreach (var kvp in uploadResults.ToList())
                {
                    var imageId = kvp.Key;
                    var file = paramsDto.Images[uploadResults.Keys.ToList().IndexOf(imageId)];
                    var uploadParams = new ImageUploadParamsDto { File = file, ImageId = imageId };
                    var result = await _imageUploader.UploadImage(uploadParams);
                    uploadResults[imageId] = result;
                }
                if (uploadResults.Count == paramsDto.AltTexts.Count)
                {
                    int index = 0;
                    foreach (var kvp in uploadResults)
                    {
                        await _createCrud.AddImage(new CreateImageCrudDto
                        {
                            Id = kvp.Key,
                            ImageUrl = kvp.Value.FilePath ?? string.Empty,
                            AltText = paramsDto.AltTexts[index],
                            NewsId = paramsDto.NewsId,
                            RelationshipId = Guid.NewGuid()
                        });
                        await _createCrud.AddNewsContentImage(new CreateContentImageCrudDto
                        {
                            ImageId = kvp.Key,
                            Id = Guid.NewGuid(),
                            BlockNumber = paramsDto.BlockNumber,
                            RelationshipId = Guid.NewGuid()
                        });
                        index++;
                }}
                return new Result
                {
                    Success = true,
                    Message = @$"Images have been added to News with ID: {paramsDto.NewsId}."
                };
            }
            catch (Exception ex)
            {
                try
                {
                    await _imageUploader.DeleteImages(new ImageDeleteParamsDto
                    {
                        ImageIds = uploadResults.Keys.ToList()
                    });
                }
                catch { }
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
        var checkResult = await _readCrud.CheckNews( new CheckNewsCrudDto { NewsId = paramsDto.NewsId } );
        if (checkResult is true)
        {
            try{
                await _createCrud.AddNewsContentText(new CreateContentTextCrudDto{
                    TextContent = paramsDto.TextContent,
                    Id = Guid.NewGuid(),
                    NewsId = paramsDto.NewsId,
                    BlockNumber = paramsDto.BlockNumber,
                    RelationshipId = Guid.NewGuid()
                });
                return new Result{Success = true, Message = "Image Added"};
            }
            catch (Exception ex)
            {
                return new Result{ Success = false, Message = $"Error: {ex}" };
            }
        }
        return new Result
        { 
            Success = false, Message = @$"
            News with title: {paramsDto.Title} and
            publish time {paramsDto.PublishDatetime} is not found"
        };
    }
}