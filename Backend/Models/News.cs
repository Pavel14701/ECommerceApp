using System;

public class News
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Content { get; set; }
    public DateTime PublishDate { get; set; }
}
