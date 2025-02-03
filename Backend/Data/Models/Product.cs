public class Product
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }
    public Guid SubcategoryId { get; set; }
    public Subcategory? Subcategory { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public required string Description { get; set; }
    public List<Images> Images { get; set; } = new List<Images>();
}