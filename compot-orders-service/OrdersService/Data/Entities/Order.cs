using OrdersService.Constraints;

namespace OrdersService.Data.Entities;

public sealed class Order
{
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required double TotalPrice { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required OrderStatus Status { get; set; }
    public List<OrderItem> Items { get; set; } = [];
}