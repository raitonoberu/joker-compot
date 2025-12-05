using Grpc.Core;
using OrdersService.Converters;
using OrdersService.Data.Entities;
using OrdersService.Protos;
using OrdersService.Repositories;
using OrderItem = OrdersService.Data.Entities.OrderItem;

namespace OrdersService.Grpc;

public sealed class OrdersGrpcService(
    IOrdersRepository ordersRepository,
    ILogger<OrdersGrpcService> logger)
    : OrdersService.Protos.OrdersService.OrdersServiceBase
{
    public override async Task<OrderResponse> CreateOrder(CreateOrderRequest request, ServerCallContext context)
    {
        try
        {
            var orderId = Guid.NewGuid();
            var order = new Order
            {
                Id = orderId,
                UserId = Guid.Parse(request.UserId),
                TotalPrice = request.Items.Sum(item => item.Price * item.Quantity),
                CreatedAt = DateTime.UtcNow,
                Status = Constraints.OrderStatus.Pending,
                Items = request.Items.Select(item => new OrderItem
                {
                    OrderId = orderId,
                    ProductId = Guid.Parse(item.ProductId),
                    Quantity = item.Quantity,
                    Price = item.Price
                }).ToList()
            };
            await ordersRepository.CreateAsync(order, context.CancellationToken);

            return order.ToProtoResponse();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating order");
            throw new RpcException(new Status(StatusCode.Internal, "Internal Server Error"));
        }
    }

    public override async Task<OrderResponse> GetOrder(GetOrderRequest request, ServerCallContext context)
    {
        try
        {
            var orderId = Guid.Parse(request.OrderId);
            var order = await ordersRepository.GetAsync(orderId)
                        ?? throw new RpcException(new Status(StatusCode.NotFound, "Order not found"));

            return order.ToProtoResponse();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting order");
            throw new RpcException(new Status(StatusCode.Internal, "Internal Server Error"));
        }
    }

    public override async Task<OrdersResponse> GetUserOrders(GetUserOrdersRequest request, ServerCallContext context)
    {
        try
        {
            var userId = Guid.Parse(request.UserId);
            var orders = await ordersRepository.GetAsync(userId, request.Offset, request.Limit);
            var ordersResponse = new OrdersResponse();
            ordersResponse.Orders.AddRange(orders.Select(x => x.ToProtoResponse()));

            return ordersResponse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user orders");
            throw new RpcException(new Status(StatusCode.Internal, "Internal Server Error"));
        }
    }

    public override async Task<OrderResponse> UpdateOrderStatus(UpdateOrderStatusRequest request,
        ServerCallContext context)
    {
        try
        {
            var orderId = Guid.Parse(request.OrderId);
            var order = await ordersRepository.GetAsync(orderId)
                        ?? throw new RpcException(new Status(StatusCode.NotFound, "Order not found"));
            var updatedOrder = await ordersRepository.UpdateStatusAsync(order.Id, request.Status.ToDomain());

            return updatedOrder!.ToProtoResponse();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating order status");
            throw new RpcException(new Status(StatusCode.Internal, "Internal Server Error"));
        }
    }
}