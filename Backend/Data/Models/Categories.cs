public class Category
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public List<Subcategory> Subcategories { get; set; } = new List<Subcategory>();
    public List<Product> Products { get; set; } = new List<Product>();
}

public class Subcategory
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }
    public List<Product> Products { get; set; } = new List<Product>();
}