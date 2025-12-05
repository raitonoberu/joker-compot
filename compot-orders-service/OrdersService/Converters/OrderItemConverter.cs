using OrdersService.Data.Entities;

namespace OrdersService.Converters;

public static class OrderItemConverter
{
    public static Protos.OrderItem ToProtoModel(this OrderItem order)
    {
        return new Protos.OrderItem
        {
            Price = order.Price,
            ProductId = order.ProductId.ToString(),
            Quantity = order.Quantity
        };
    }
}