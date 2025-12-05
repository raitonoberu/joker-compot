namespace OrdersService.Data.Entities;

public sealed class OrderItem
{
    public required Guid OrderId { get; set; }
    public required Guid ProductId { get; set; }
    public required int Quantity { get; set; }
    public required double Price { get; set; }
}