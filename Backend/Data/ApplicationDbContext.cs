using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Images> Images { get; set; }
    public DbSet<NewsImageRelationship> NewsImageRelationships { get; set; }
    public DbSet<ProductImageRelationship> ProductImageRelationships { get; set; }
    public DbSet<News> News { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Subcategory> Subcategories { get; set; }
    public DbSet<CategoriesRelationship> CategoriesRelationships { get; set; }
    public DbSet<SubcategoriesRelationship> SubcategoriesRelationships { get; set; }
    public DbSet<Content> Contents { get; set; }
    public DbSet<NewsRelationships> NewsRelationships { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<OrderItemRelationship> OrderItemRelationships { get; set; }
    public DbSet<OrderDiscountRelationship> OrderDiscountRelationships { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>().ToTable("users");

        // Images Configuration
        modelBuilder.Entity<Images>()
            .HasOne(i => i.NewsImageRelationship)
            .WithOne(nir => nir.Image)
            .HasForeignKey<NewsImageRelationship>(nir => nir.ImageId);

        modelBuilder.Entity<Images>()
            .HasOne(i => i.ProductImageRelationship)
            .WithOne(pir => pir.Image)
            .HasForeignKey<ProductImageRelationship>(pir => pir.ImageId);

        // NewsImageRelationship Configuration
        modelBuilder.Entity<NewsImageRelationship>()
            .HasOne(nir => nir.Image)
            .WithOne(i => i.NewsImageRelationship)
            .HasForeignKey<NewsImageRelationship>(nir => nir.ImageId);

        modelBuilder.Entity<NewsImageRelationship>()
            .HasOne(nir => nir.News)
            .WithMany(n => n.NewsImageRelationships)
            .HasForeignKey(nir => nir.NewsId);

        // ProductImageRelationship Configuration
        modelBuilder.Entity<ProductImageRelationship>()
            .HasOne(pir => pir.Image)
            .WithOne(i => i.ProductImageRelationship)
            .HasForeignKey<ProductImageRelationship>(pir => pir.ImageId);

        modelBuilder.Entity<ProductImageRelationship>()
            .HasOne(pir => pir.Product)
            .WithMany(p => p.ProductImageRelationships)
            .HasForeignKey(pir => pir.ProductId);

        // Category Configuration
        modelBuilder.Entity<Category>()
            .HasMany(c => c.CategoriesRelationships)
            .WithOne(cr => cr.Category)
            .HasForeignKey(cr => cr.CategoryId);

        modelBuilder.Entity<Category>()
            .HasMany(c => c.SubcategoriesRelationships)
            .WithOne(sr => sr.Category)
            .HasForeignKey(sr => sr.CategoryId);

        // Subcategory Configuration
        modelBuilder.Entity<Subcategory>()
            .HasMany(s => s.CategoriesRelationships)
            .WithOne(cr => cr.Subcategory)
            .HasForeignKey(cr => cr.SubcategoryId);

        modelBuilder.Entity<Subcategory>()
            .HasMany(s => s.SubcategoriesRelationships)
            .WithOne(sr => sr.Subcategory)
            .HasForeignKey(sr => sr.SubcategoryId);

        // CategoriesRelationship Configuration
        modelBuilder.Entity<CategoriesRelationship>()
            .HasOne(cr => cr.Category)
            .WithMany(c => c.CategoriesRelationships)
            .HasForeignKey(cr => cr.CategoryId);

        modelBuilder.Entity<CategoriesRelationship>()
            .HasOne(cr => cr.Subcategory)
            .WithMany(s => s.CategoriesRelationships)
            .HasForeignKey(cr => cr.SubcategoryId);

        modelBuilder.Entity<CategoriesRelationship>()
            .HasOne(cr => cr.Product)
            .WithMany(p => p.CategoriesRelationships)
            .HasForeignKey(cr => cr.ProductId);

        // SubcategoriesRelationship Configuration
        modelBuilder.Entity<SubcategoriesRelationship>()
            .HasOne(sr => sr.Category)
            .WithMany(c => c.SubcategoriesRelationships)
            .HasForeignKey(sr => sr.CategoryId);

        modelBuilder.Entity<SubcategoriesRelationship>()
            .HasOne(sr => sr.Subcategory)
            .WithMany(s => s.SubcategoriesRelationships)
            .HasForeignKey(sr => sr.SubcategoryId);

        modelBuilder.Entity<SubcategoriesRelationship>()
            .HasOne(sr => sr.Product)
            .WithOne(p => p.SubcategoriesRelationship)
            .HasForeignKey<SubcategoriesRelationship>(sr => sr.ProductId);

        // Content Configuration
        modelBuilder.Entity<Content>()
            .HasMany(c => c.NewsRelationships)
            .WithOne(nr => nr.Content)
            .HasForeignKey(nr => nr.ContentId);

        // News Configuration
        modelBuilder.Entity<News>()
            .HasMany(n => n.NewsImageRelationships)
            .WithOne(nir => nir.News)
            .HasForeignKey(nir => nir.NewsId);

        modelBuilder.Entity<News>()
            .HasMany(n => n.NewsRelationships)
            .WithOne(nr => nr.News)
            .HasForeignKey(nr => nr.NewsId);

        // NewsRelationships Configuration
        modelBuilder.Entity<NewsRelationships>()
            .HasOne(nr => nr.Content)
            .WithMany(c => c.NewsRelationships)
            .HasForeignKey(nr => nr.ContentId);

        modelBuilder.Entity<NewsRelationships>()
            .HasOne(nr => nr.News)
            .WithMany(n => n.NewsRelationships)
            .HasForeignKey(nr => nr.NewsId);

        // Order Configuration
        modelBuilder.Entity<Order>()
            .HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.OrderDiscounts)
            .WithOne(od => od.Order)
            .HasForeignKey(od => od.OrderId);

        // OrderItem Configuration
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.OrderItemRelationships)
            .WithOne(o => o.OrderItem)
            .HasForeignKey<OrderItemRelationship>(oir => oir.OrderItemId);

        // Discount Configuration
        modelBuilder.Entity<Discount>()
            .HasOne(d => d.OrderDiscountRelationships)
            .WithOne(o => o.Discount)
            .HasForeignKey<OrderDiscountRelationship>(odr => odr.DiscountId);

        // OrderItemRelationship Configuration
        modelBuilder.Entity<OrderItemRelationship>()
            .HasOne(oir => oir.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oir => oir.OrderId);

        modelBuilder.Entity<OrderItemRelationship>()
            .HasOne(oir => oir.OrderItem)
            .WithOne(oi => oi.OrderItemRelationships)
            .HasForeignKey<OrderItemRelationship>(oir => oir.OrderItemId);

        // OrderDiscountRelationship Configuration
        modelBuilder.Entity<OrderDiscountRelationship>()
            .HasOne(odr => odr.Order)
            .WithMany(o => o.OrderDiscounts)
            .HasForeignKey(odr => odr.OrderId);

        modelBuilder.Entity<OrderDiscountRelationship>()
            .HasOne(odr => odr.Discount)
            .WithOne(d => d.OrderDiscountRelationships)
            .HasForeignKey<OrderDiscountRelationship>(odr => odr.DiscountId);
    }
}