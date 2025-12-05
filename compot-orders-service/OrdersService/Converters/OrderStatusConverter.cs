using OrdersService.Constraints;

namespace OrdersService.Converters;

public static class OrderStatusConverter
{
    public static Protos.OrderStatus ToProto(this OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Unknown => Protos.OrderStatus.Unspecified,
            OrderStatus.Pending => Protos.OrderStatus.Pending,
            OrderStatus.Paid => Protos.OrderStatus.Paid,
            OrderStatus.Shipped => Protos.OrderStatus.Shipped,
            OrderStatus.Cancelled => Protos.OrderStatus.Cancelled,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    public static OrderStatus ToDomain(this Protos.OrderStatus status)
    {
        return status switch
        {
            Protos.OrderStatus.Unspecified => OrderStatus.Unknown,
            Protos.OrderStatus.Pending => OrderStatus.Pending,
            Protos.OrderStatus.Paid => OrderStatus.Paid,
            Protos.OrderStatus.Shipped => OrderStatus.Shipped,
            Protos.OrderStatus.Cancelled => OrderStatus.Cancelled,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}