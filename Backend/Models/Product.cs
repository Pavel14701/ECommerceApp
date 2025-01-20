using System;
using System.Collections.Generic;

public class Product
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public required string Description { get; set; }
    public List<Images> Images { get; set; } = new List<Images>();
}
