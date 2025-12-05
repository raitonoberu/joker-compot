using Microsoft.EntityFrameworkCore;
using OrdersService.Data.Entities;

namespace OrdersService.Data;

public sealed class OrdersContext(DbContextOptions<OrdersContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<OrderItem>(e =>
            {
                e.HasKey(x => new { x.OrderId, x.ProductId });
            }
        );
        b.Entity<Order>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.UserId);
                e.HasMany<OrderItem>(x => x.Items).WithOne().HasForeignKey(x => x.OrderId);
            }
        );
    }
}