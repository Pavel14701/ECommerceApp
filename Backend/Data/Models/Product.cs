using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

[Table("products")]
[Index(nameof(Id))]
[Index(nameof(Name))]
[Index(nameof(Price))]
public class Product
{
    [Key]
    [Column("id", TypeName = "uuid")]
    public Guid Id { get; set; }

    [Required]
    [Column("name", TypeName = "varchar(255)")]
    public required string Name { get; set; }


    [Column("price", TypeName = "numeric(10,2)")]
    public decimal Price { get; set; }

    [Column("stock", TypeName = "integer")]
    public int Stock { get; set; }

    [Required]
    [Column("description", TypeName = "varchar(2048)")]
    public required string Description { get; set; }

    [Column("discount", TypeName = "integer")]
    [Range(1, 99, ErrorMessage = "Discount must be between 1 and 99.")]
    public int? Discount { get; set; }

    [InverseProperty("Product")]
    public required List<CategoriesRelationship> CategoriesRelationships { get; set; }

    [InverseProperty("Product")]
    public required List<ProductImageRelationship> ProductImageRelationships { get; set; }

    [InverseProperty("Product")]
    public required List<Category> Category { get; set; }

    [InverseProperty("Product")]
    public required List<Subcategory> Subcategory { get; set; }
}