using Microsoft.EntityFrameworkCore;
using OrdersService.Constraints;
using OrdersService.Data;
using OrdersService.Data.Entities;

namespace OrdersService.Repositories;

public sealed class OrdersRepository(OrdersContext context) : IOrdersRepository
{
    public async Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order);
        await context.AddAsync(order, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return order;
    }

    public Task<Order?> GetAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return context
            .Orders
            .Include(x => x.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);
    }

    public Task<List<Order>> GetAsync(
        Guid userId,
        int offset,
        int limit,
        CancellationToken cancellationToken = default)
    {
        return context
            .Orders
            .Include(x => x.Items)
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<Order?> UpdateStatusAsync(
        Guid orderId,
        OrderStatus status,
        CancellationToken cancellationToken = default)
    {
        var entity = await context.Orders.FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);
        if (entity is null)
            return null;

        entity.Status = status;
        context.Orders.Update(entity);
        return entity;
    }
}