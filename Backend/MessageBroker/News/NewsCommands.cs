public class GetAllNewsQuery
{
    public Guid QueryId { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class GetNewsByIdQuery
{
    public Guid QueryId { get; set; }
    public Guid NewsId { get; set; }
}


public class UploadImageCommand
{
    public Guid CommandId { get; set; }
    public Guid Id { get; set; }
    public required IFormFile File { get; set; }
}


public class AddImageToNewsCommand
{
    public Guid CommandId { get; set; }
    public Guid NewsId { get; set; }
    public required Images Image { get; set; }
}


public class CreateNewsCommand
{
    public Guid CommandId { get; set; }
    public required News News { get; set; }
}

public class DeleteNewsCommand
{
    public Guid CommandId { get; set; }
    public Guid NewsId { get; set; }
}

public class UpdateNewsCommand
{
    public Guid CommandId { get; set; }
    public required News News { get; set; }
}

public class UpdateImageCommand
{
    public Guid CommandId { get; set; }
    public Guid NewsId { get; set; }
    public Guid ImageId { get; set; }
    public required IFormFile File { get; set; }
}

public class UpdateNewsTitleCommand
{
    public Guid CommandId { get; set; }
    public Guid NewsId { get; set; }
    public required string Title { get; set; }
}

public class UpdateNewsPublishDateCommand
{
    public Guid CommandId { get; set; }
    public Guid NewsId { get; set; }
    public DateTime PublishDate { get; set; }
}

public class UpdateNewsContentTextCommand
{
    public Guid CommandId { get; set; }
    public Guid NewsId { get; set; }
    public Guid ContentId { get; set; }
    public required string Text { get; set; }
}

