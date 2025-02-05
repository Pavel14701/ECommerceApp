public class ImageUplodedDto
{
    public Guid? ImageId { get; set; } = Guid.Empty;
    public string?  FilePath { get; set; } = string.Empty;
    public bool? Success { get; set; } = null;
}

public class ImageDeleteResultDto
{
    public bool? Success { get; set; } = null;
}



public class ImageUploadParamsDto
{
    public required Guid ImageId { get; set; }
    public required IFormFile File { get; set;}
}



public class ImageDeleteParamsDto
{
    public required List<Guid> ImageIds { get; set; }
}



public interface IImageUploader
{
    Task<ImageUplodedDto> UploadImage(ImageUploadParamsDto uploadParams);
    Task<ImageDeleteResultDto> DeleteImages(ImageDeleteParamsDto deleteParams);
}



public class ImageUploader : IImageUploader
{
    private readonly string _uploadPath;

    public ImageUploader()
    {
        _uploadPath = Path.Combine(
            Directory.GetCurrentDirectory(), "wwwroot", "uploads"
        );
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<ImageUplodedDto> UploadImage(ImageUploadParamsDto uploadParams)
    {   
        try
        {
            if (uploadParams.File == null || uploadParams.File.Length == 0)
            {
                throw new FileLoadException("Invalid product ID or file.");
            }
            var fileName = $"{uploadParams.ImageId}_{uploadParams.File.FileName}";
            var filePath = Path.Combine(_uploadPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await uploadParams.File.CopyToAsync(stream);
            }
            return new ImageUplodedDto
            {
                Success = true,
                FilePath = filePath,
                ImageId = uploadParams.ImageId 
            };
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<ImageDeleteResultDto> DeleteImages(ImageDeleteParamsDto deleteParams)
    {
        try
        {
            var filePatterns = deleteParams.ImageIds.Select(id => $"{id}_*").ToList();
            bool anyFilesDeleted = false;
            foreach (var pattern in filePatterns)
            {
                var files = await Task.Run(() => Directory.GetFiles(_uploadPath, pattern));
                if (files.Length > 0)
                {
                    anyFilesDeleted = true;
                    foreach (var file in files)
                    {
                        await Task.Run(() => File.Delete(file));
                    }
                }
            }
            if (!anyFilesDeleted)
            {
                return new ImageDeleteResultDto
                {
                    Success = false,
                };
            }
            return new ImageDeleteResultDto
            {
                Success = true,
            };
        }
        catch (Exception)
        {
            throw;
        }
    }
}
