using Microsoft.EntityFrameworkCore;

public class ApplicationAdminDbContext : DbContext
{
    public ApplicationAdminDbContext(DbContextOptions<ApplicationAdminDbContext> options) : base(options) { }

    public DbSet<ApplicationAdmin> ApplicationAdmin { get; set; } = null!;
}
