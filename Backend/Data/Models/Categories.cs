using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



[Table("categories")]
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

    [InverseProperty("Category")]
    public List<SubcategoriesRelationship> SubcategoriesRelationships { get; set; } = new List<SubcategoriesRelationship>();
}




[Table("subcategories")]
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

    [InverseProperty("Subcategory")]
    public List<SubcategoriesRelationship> SubcategoriesRelationships { get; set; } = new List<SubcategoriesRelationship>();
}






[Table("categories_relationship")]
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






[Table("subcategories_relationship")]
public class SubcategoriesRelationship
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

    [InverseProperty("SubcategoriesRelationship")]
    public required Category Category { get; set; }

    [InverseProperty("SubcategoriesRelationship")]
    public required Subcategory Subcategory { get; set; }

    [InverseProperty("SubcategoriesRelationship")]
    public required Product Product { get; set; }
}
