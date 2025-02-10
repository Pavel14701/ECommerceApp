public class UpdateNewsImageImageBlock
{
    public required Guid Id { get; set; }
    public required int BlockNumber { get; set; }
    public required List<IFormFile> Images { get; set; }
}

public class UpdateNewsPreviewDto
{
    public required Guid Id { get; set; }
    public IFormFile? Image { get; set; }
    public string? Title { get; set; }
    public string? Text { get; set; }
}


public interface IUpdateNewsService
{
    Task<Result> UpdateNewsPreview(UpdateNewsPreviewDto paramsDto);
    Task<Result> UpdateNewsImageBlock(UpdateNewsImageImageBlock paramsDto);
    Task<Result> UpdateNewsContentBlock(UpdateNewsTextParamsDto paramsDto);
}


public class UpdateNewsService : IUpdateNewsService
{
    private readonly ImageUploader _imageUploader;
    private readonly UpdateCrud _updateCrud;
    private readonly ReadCrud _readCrud;

    public UpdateNewsService(
        ImageUploader imageUploader,
        UpdateCrud updateCrud,
        ReadCrud readCrud
    )
    {
        _imageUploader = imageUploader;
        _updateCrud = updateCrud;
        _readCrud = readCrud;
    }


    public async Task<Result> UpdateNewsPreview(UpdateNewsPreviewDto paramsDto)
    {
        var newImageId = Guid.NewGuid();
        var newImages = new Dictionary<Guid, string?>();
        var oldImages = new List<Guid>();
        try{
            if (paramsDto.Image is not null)
            {
                
                var result = await _imageUploader.UploadImage(new ImageUploadParamsDto
                {
                    ImageId = newImageId,
                    File = paramsDto.Image
                });
                newImages.Add(newImageId, result.FilePath?? throw new Exception());
            }
            oldImages = await _updateCrud.UpdateNewsPreview(new UpdateNewsPreviewParamsDto
            {
                NewsId = paramsDto.Id,
                NewTitle = paramsDto.Title,
                NewImageUrl = newImages[newImageId],
                NewImageId = newImageId,
                NewText = paramsDto.Text
            });
            if (newImages[newImageId] is not null)
            {
                await _imageUploader.DeleteImages(new ImageDeleteParamsDto{ImageIds = oldImages});
            }
            return new Result
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            try{
                await _imageUploader.DeleteImages(
                    new ImageDeleteParamsDto{ImageIds = newImages.Keys.ToList()});
            }catch{}
            return new Result
            {
                Success = false,
                Message = $"Error: {ex}"
            };
        }
    }


    public async Task<Result> UpdateNewsImageBlock(UpdateNewsImageImageBlock paramsDto)
    {
        var newImagesFileDict = new Dictionary<Guid, IFormFile>();
        var newImagesDict = new Dictionary<Guid, string>();
        try{
            foreach (var file in paramsDto.Images)
            {
                var newImageId = Guid.NewGuid();
                newImagesFileDict.Add(newImageId, file);
                newImagesDict.Add(newImageId, string.Empty);
            }
            foreach (var kvp in newImagesFileDict)
            {
                var result = await _imageUploader.UploadImage
                (
                    new ImageUploadParamsDto{ ImageId = kvp.Key, File = kvp.Value }
                );
                newImagesDict[kvp.Key] = (result.FilePath != string.Empty, null) ? result.FilePath : throw new Exception("Result error");
            }
            newImagesFileDict = null;
            await _updateCrud.UpdateNewsImagesByBlockNumber
            (
                new UpdateNewsImagesParamsDto
                {
                    NewsId = paramsDto.Id,
                    ImagesData = newImagesDict,
                    BlockNumber = paramsDto.BlockNumber
                }
            );
            return new Result
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            try{
                var ImgIds = newImagesDict.Keys.ToList();
                await _imageUploader.DeleteImages(new ImageDeleteParamsDto{ ImageIds=ImgIds });
            }catch{}
            return new Result
            {
                Success = false,
                Message = $"Error: {ex}"
            };
        }
    }

    public async Task<Result> UpdateNewsContentBlock(UpdateNewsTextParamsDto paramsDto)
    {
        try{
            await _updateCrud.UpdateNewsTextBlock( paramsDto );
            return new Result{ Success = true };
        }
        catch (Exception ex)
        { return new Result { Success = false, Message = $"Error: {ex}" }; }
    }
}