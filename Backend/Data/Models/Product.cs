using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

[Table("products")]
[Index(nameof(Id))]
[Index(nameof(CategoryId))]
[Index(nameof(SubcategoryId))]
public class Product
{
    [Key]
    [Column("id", TypeName = "uuid")]
    public Guid Id { get; set; }

    [Required]
    [Column("name", TypeName = "varchar(255)")]
    public required string Name { get; set; }

    [Required]
    [ForeignKey("CategoryId")]
    [Column("fk_category", TypeName = "uuid")]
    public Guid CategoryId { get; set; }

    [Required]
    [ForeignKey("SubcategoryId")]
    [Column("fk_subcategory", TypeName = "uuid")]
    public Guid SubcategoryId { get; set; }

    [Column("price", TypeName = "numeric(10,2)")]
    public decimal Price { get; set; }

    [Column("stock", TypeName = "integer")]
    public int Stock { get; set; }

    [Required]
    [Column("description", TypeName = "varchar(2048)")]
    public required string Description { get; set; }

    [InverseProperty("Product")]
    public List<CategoriesRelationship> CategoriesRelationships { get; set; } = new List<CategoriesRelationship>();

    [InverseProperty("Product")]
    public List<ProductImageRelationship> ProductImageRelationships { get; set; } = new List<ProductImageRelationship>();

    [InverseProperty("Product")]
    public required Category Category { get; set; }

    [InverseProperty("Product")]
    public required Subcategory Subcategory { get; set; }
}
