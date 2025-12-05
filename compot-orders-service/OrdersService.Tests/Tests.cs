using Grpc.Net.Client;
using NUnit.Framework;
using OrdersService.Protos;
using OrdersClient = OrdersService.Protos.OrdersService.OrdersServiceClient;

[assembly: Parallelizable(ParallelScope.All)]

namespace OrderServices.Tests;

public class Tests
{
    private readonly OrdersClient _client = new(GrpcChannel.ForAddress("http://localhost:6000"));

    [Test]
    public void CreateItem()
    {
        var request = GetCreateOrderRequest();
        var response = _client.CreateOrder(request);

        Assert.That(request.UserId, Is.EqualTo(response.UserId));
        Assert.That(request.Items.Count, Is.EqualTo(request.Items.Count));
        Assert.That(request.Items, Is.EquivalentTo(response.Items));
        Assert.That(response.TotalPrice, Is.Not.Default);
    }

    [Test]
    public void CreateItem_ThenUpdateStatus()
    {
        var request = GetCreateOrderRequest();
        var order = _client.CreateOrder(request);
        
        Assert.That(order.Status == OrderStatus.Pending);
        
        var changeStatusRequest = new UpdateOrderStatusRequest
        {
            Status = OrderStatus.Paid,
            OrderId = order.Id
        };
        var response = _client.UpdateOrderStatus(changeStatusRequest);
        
        Assert.That(order.Id == response.Id);
        Assert.That(response.Status == OrderStatus.Paid);
    }

    [Test]
    public void QueryUserItems()
    {
        var userId = Guid.NewGuid();
        for (var i = 0; i < 5; i++)
        {
            var request = GetCreateOrderRequest(userId);
            _client.CreateOrder(request);
        }

        var response = _client.GetUserOrders(new GetUserOrdersRequest
        {
            Limit = 10,
            Offset = 0,
            UserId = userId.ToString()
        });
        
        Assert.That(response.Orders.Count, Is.EqualTo(5));
        Assert.That(response.Orders.Select(x => x.UserId), Is.All.EqualTo(userId.ToString()));
    }

    private CreateOrderRequest GetCreateOrderRequest(Guid? userId = null)
    {
        var request = new CreateOrderRequest
        {
            UserId = userId?.ToString() ?? Guid.NewGuid().ToString()
        };
        request.Items.AddRange([
            new OrderItem
            {
                Price = 3213,
                ProductId = Guid.NewGuid().ToString(),
                Quantity = 2
            },
            new OrderItem
            {
                Price = 6151,
                ProductId = Guid.NewGuid().ToString(),
                Quantity = 6
            }
        ]);

        return request;
    }
}