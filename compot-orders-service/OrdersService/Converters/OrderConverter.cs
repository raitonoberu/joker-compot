using OrdersService.Data.Entities;

namespace OrdersService.Converters;

public static class OrderConverter
{
    public static Protos.OrderResponse ToProtoResponse(this Order order)
    {
        var response = new Protos.OrderResponse
        {
            Id = order.Id.ToString(),
            UserId = order.UserId.ToString(),
            TotalPrice = Convert.ToDouble(order.TotalPrice),
            CreatedAt = order.CreatedAt.ToString("O"),
            Status = order.Status.ToProto()
        };
        response.Items.AddRange(order.Items.Select(x => x.ToProtoModel()));

        return response;
    }
}