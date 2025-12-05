using OrdersService.Constraints;
using OrdersService.Data.Entities;

namespace OrdersService.Repositories;

public interface IOrdersRepository
{
    Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default);

    Task<Order?> GetAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task<List<Order>> GetAsync(
        Guid userId,
        int offset,
        int limit,
        CancellationToken cancellationToken = default);

    Task<Order?> UpdateStatusAsync(
        Guid orderId,
        OrderStatus status,
        CancellationToken cancellationToken = default);
}