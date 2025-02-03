using System;
using System.Collections.Generic;

public class Content
{
    public Guid Id { get; set; }
    public required string Text { get; set;}
}

public class News
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public List<Images> Images { get; set; } = new List<Images>();
    public List<Content> Content { get; set; } = new List<Content>();
    public DateTime PublishDate { get; set; }
}
