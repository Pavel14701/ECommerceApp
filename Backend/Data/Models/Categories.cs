using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;



[Table("categories")]
[Index(nameof(Id))]
public class Category
{
    [Key]
    [Column("id", TypeName = "uuid")]
    public Guid Id { get; set; }

    [Required]
    [Column("category_name", TypeName = "varchar(255)")]
    public required string Name { get; set; }

    [InverseProperty("Category")]
    public List<CategoriesRelationship> CategoriesRelationships { get; set; } = new List<CategoriesRelationship>();
}




[Table("subcategories")]
[Index(nameof(Id))]
public class Subcategory
{
    [Key]
    [Column("id", TypeName = "uuid")]
    public Guid Id { get; set; }

    [Required]
    [Column("subcategory_name", TypeName = "varchar(255)")]
    public required string Name { get; set; }

    [InverseProperty("Subcategory")]
    public List<CategoriesRelationship> CategoriesRelationships { get; set; } = new List<CategoriesRelationship>();

}



[Table("category_relationship")]
[Index(nameof(CategoryId))]
[Index(nameof(SubcategoryId))]
[Index(nameof(ProductId))]
public class CategoriesRelationship
{
    [Key]
    [Column("id", TypeName = "uuid")]
    public Guid Id { get; set; }

    [ForeignKey("CategoryId")]
    [Column("fk_category", TypeName = "uuid")]
    public required Guid CategoryId { get; set; }

    [ForeignKey("SubcategoryId")]
    [Column("fk_subcategory", TypeName = "uuid")]
    public required Guid SubcategoryId { get; set; }

    [ForeignKey("ProductId")]
    [Column("fk_product", TypeName = "uuid")]
    public required Guid ProductId { get; set; }

    [InverseProperty("CategoriesRelationships")]
    public required Category Category { get; set; }

    [InverseProperty("CategoriesRelationships")]
    public required Subcategory Subcategory { get; set; }

    [InverseProperty("CategoriesRelationships")]
    public required Product Product { get; set; }
}
