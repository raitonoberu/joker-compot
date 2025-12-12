using Microsoft.EntityFrameworkCore;
using ProductsService.Data.Entities;

namespace ProductsService.Data;

public sealed class ProductsContext(DbContextOptions<ProductsContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<Product>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasOne<Category>(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId);
            }
        );
        b.Entity<Category>(e =>
            {
                e.HasKey(x => x.Id);
            }
        );
    }
}