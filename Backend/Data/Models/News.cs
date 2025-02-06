using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

public interface ITrackable
{
    DateTime PublishDatetime { get; set; }
    DateTime UpdateDatetime { get; set; }
}

[Table("news_content")]
[Index(nameof(BlockNumber))]
public class Content
{
    [Key]
    [Column("id", TypeName = "uuid")]
    public required Guid Id { get; set; }

    [Column("text", TypeName = "varchar(4096)")]
    public string? Text { get; set; }

    [Required]
    [Column("block_number")]
    public required int BlockNumber { get; set; }

    [ForeignKey(nameof(NewsId))]
    [Column("fk_news_id", TypeName = "uuid")]
    public Guid? NewsId { get; set; }
    public News? News { get; set; }

    [InverseProperty("Content")]
    public List<NewsRelationships> NewsRelationships { get; set; } = new List<NewsRelationships>();
}

[Table("news")]
[Index(nameof(Title))]
[Index(nameof(PublishDatetime))]
public class News : ITrackable
{
    [Key]
    [Column("id", TypeName = "uuid")]
    public Guid Id { get; set; }

    [Required]
    [Column("news_title", TypeName = "varchar(255)")]
    public required string Title { get; set; }

    [Column("publish_datetime", TypeName = "timestamp")]
    public DateTime PublishDatetime { get; set; }

    [Column("update_datetime", TypeName = "timestamp")]
    public DateTime UpdateDatetime { get; set; }

    [InverseProperty("News")]
    public List<NewsImageRelationship> NewsImageRelationships { get; set; } = new List<NewsImageRelationship>();

    [InverseProperty("News")]
    public List<NewsRelationships> NewsRelationships { get; set; } = new List<NewsRelationships>();
}

[Table("news_relationships")]
[Index(nameof(NewsId))]
[Index(nameof(ContentId))]
public class NewsRelationships
{
    [Key]
    [Column("id", TypeName = "uuid")]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey("ContentId")]
    [Column("fk_content", TypeName = "uuid")]
    public required Guid ContentId { get; set; }

    [Required]
    [ForeignKey("NewsId")]
    [Column("fk_news", TypeName = "uuid")]
    public required Guid NewsId { get; set; }

    [InverseProperty("NewsRelationships")]
    public required Content Content { get; set; }

    [InverseProperty("NewsRelationships")]
    public required News News { get; set; }
}
