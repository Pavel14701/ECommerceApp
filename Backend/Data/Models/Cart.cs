using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

[Table("cart")]
[Index(nameof(UserId))]
public class Cart
{
    [Key]
    [Column("id", TypeName = "uuid")]
    public required Guid Id { get; set; }

    [Column("id", TypeName = "uuid")]
    public required Guid UserId { get; set; }
}

[Table("cart_relationship")]
[Index(nameof(CartId))]
public class CartRelationship
{
    [Key]
    [Column("id", TypeName = "uuid")]
    public required Guid Id { get; set; }

    [ForeignKey("CartId")]
    [Column("fk_cart_id", TypeName = "uuid")]
    public required Guid CartId { get; set; }

    [ForeignKey("ProductId")]
    [Column("fk_product_id", TypeName = "uuid")]
    public required Guid ProductId { get; set; }

    [Column("count", TypeName = "integer(3)")]
    public required int Count { get; set; }
}