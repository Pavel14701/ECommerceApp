using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;




[Table("orders")]
public class Order
{
    [Key]
    [Column("id", TypeName = "uuid")]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey("UserId")]
    [Column("user_id", TypeName = "uuid")]
    public Guid UserId { get; set; }

    [Required]
    [Column("order_date", TypeName = "timestamp")]
    public DateTime OrderDate { get; set; }

    [Column("total_amount", TypeName = "numeric(10,2)")]
    public decimal TotalAmount { get; private set; }

    [InverseProperty("Order")]
    public List<OrderItemRelationship> OrderItems { get; set; } = new List<OrderItemRelationship>();

    [InverseProperty("Order")]
    public List<OrderDiscountRelationship> OrderDiscounts { get; set; } = new List<OrderDiscountRelationship>();

    public void CalculateTotalAmount()
    {
        decimal total = 0;

        foreach (var item in OrderItems)
        {
            total += item.OrderItem.Quantity * item.OrderItem.UnitPrice;
        }

        foreach (var discount in OrderDiscounts)
        {
            total -= discount.Discount.Amount;
        }

        TotalAmount = total;
    }
}



[Table("order_items")]
public class OrderItem
{
    [Key]
    [Column("id", TypeName = "uuid")]
    public Guid Id { get; set; }

    [Required]
    [Column("quantity", TypeName = "integer")]
    public int Quantity { get; set; }

    [Column("unit_price", TypeName = "numeric(10,2)")]
    public decimal UnitPrice { get; set; }

    [InverseProperty("OrderItem")]
    public required OrderItemRelationship OrderItemRelationships { get; set; }
}



[Table("discounts")]
public class Discount
{
    [Key]
    [Column("id", TypeName = "uuid")]
    public Guid Id { get; set; }

    [Required]
    [Column("code", TypeName = "varchar(50)")]
    public required string Code { get; set; }

    [Required]
    [Column("amount", TypeName = "numeric(10,2)")]
    public decimal Amount { get; set; }

    [InverseProperty("Discount")]
    public required OrderDiscountRelationship OrderDiscountRelationships { get; set; }
}




[Table("order_items_relationship")]
public class OrderItemRelationship
{
    [Key]
    [Column("id", TypeName = "uuid")]
    public Guid Id { get; set; }

    [ForeignKey("OrderId")]
    [Column("order_id", TypeName = "uuid")]
    public Guid OrderId { get; set; }

    [ForeignKey("OrderItemId")]
    [Column("order_item_id", TypeName = "uuid")]
    public Guid OrderItemId { get; set; }

    [InverseProperty("OrderItems")]
    public required Order Order { get; set; }

    [InverseProperty("OrderItemRelationships")]
    public required OrderItem OrderItem { get; set; }
}





[Table("order_discounts_relationship")]
public class OrderDiscountRelationship
{
    [Key]
    [Column("id", TypeName = "uuid")]
    public Guid Id { get; set; }

    [ForeignKey("OrderId")]
    [Column("order_id", TypeName = "uuid")]
    public Guid OrderId { get; set; }

    [ForeignKey("DiscountId")]
    [Column("discount_id", TypeName = "uuid")]
    public Guid DiscountId { get; set; }

    [InverseProperty("OrderDiscounts")]
    public required Order Order { get; set; }

    [InverseProperty("OrderDiscountRelationships")]
    public required Discount Discount { get; set; }
}
