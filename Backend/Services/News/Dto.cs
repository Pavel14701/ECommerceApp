public class PagedNewsDto
{
    public IEnumerable<News>? News { get; set; }
    public int TotalCount { get; set; }
}

public class ImageUploadResultDto
{
    public Guid? ImageId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

}

public class NewsCreationResultDto
{
    public Guid NewsId { get; set; }
    public required string Message { get; set; }
}
public class NewsDto
{
    public News? News { get; set; }
}

public class NewsDeletionResultDto
{
    public bool Success { get; set; }
    public required string Message { get; set; }
}

public class NewsUpdateResultDto
{
    public bool Success { get; set; }
    public required string Message { get; set; }
}