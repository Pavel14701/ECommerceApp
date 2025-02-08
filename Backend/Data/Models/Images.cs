using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;


[Table("images")]
[Index(nameof(Id))]
public class Images
{
    [Key]
    [Column("id", TypeName = "uuid")]
    public Guid Id { get; set; }

    [Required]
    [Column("image_url", TypeName = "varchar(512)")]
    public required string ImageUrl { get; set; }

    [Required]
    [Column("alt", TypeName = "varchar(255)")]
    public required string Alt { get; set; }

    [InverseProperty("Image")]
    public NewsImageRelationship? NewsImageRelationship { get; set; }

    [InverseProperty("Image")]
    public ProductImageRelationship? ProductImageRelationship { get; set; }
}




[Table("news_images_relationship")]
[Index(nameof(ImageId))]
[Index(nameof(NewsId))]
public class NewsImageRelationship
{
    [Key]
    [Column("id", TypeName = "uuid")]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey("ImageId")]
    [Column("image_id", TypeName = "uuid")]
    public required Guid ImageId { get; set; }

    [Required]
    [ForeignKey("NewsId")]
    [Column("fk_news_id", TypeName = "uuid")]
    public required Guid NewsId { get; set; }

    [InverseProperty("NewsImageRelationships")]
    public required Images Image { get; set; }

    [InverseProperty("NewsImageRelationships")]
    public required News News { get; set; }
}




[Table("product_image_relationship")]
[Index(nameof(ProductId))]
[Index(nameof(ImageId))]
public class ProductImageRelationship
{
    [Key]
    [Column("id", TypeName = "uuid")]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey("ImageId")]
    [Column("image_id", TypeName = "uuid")]
    public required Guid ImageId { get; set; }

    [Required]
    [ForeignKey("ProductId")]
    [Column("fk_product_id", TypeName = "uuid")]
    public required Guid ProductId { get; set; }

    [InverseProperty("ProductImageRelationships")]
    public required Images Image { get; set; }

    [InverseProperty("ProductImageRelationships")]
    public required Product Product { get; set; }
}
