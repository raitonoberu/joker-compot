using UsersService.Protos;
using OrdersService.Protos;
using ProductsService.Protos;

namespace CompotGateway.Models;

public class UserProfile
{
    public UserResponse? User { get; set; }
    public IEnumerable<OrderResponse>? RecentOrders { get; set; }
    public IEnumerable<ProductResponse>? FeaturedProducts { get; set; }
}

