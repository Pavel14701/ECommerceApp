using System;
using System.Collections.Generic;

public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; private set; }
    public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    public List<Discount> Discounts { get; set; } = new List<Discount>();

    public void CalculateTotalAmount()
    {
        decimal total = 0;

        foreach (var item in Items)
        {
            total += item.Quantity * item.UnitPrice;
        }

        foreach (var discount in Discounts)
        {
            total -= discount.Amount;
        }

        TotalAmount = total;
    }
}

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class Discount
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public decimal Amount { get; set; }
}
