using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using UsersService.Protos;
using OrdersService.Protos;
using ProductsService.Protos;
using CompotGateway.Models;

namespace CompotGateway.Services;

public class ProfileAggregator
{
    private readonly InfoService.InfoServiceClient _infoClient;
    private readonly OrdersService.Protos.OrdersService.OrdersServiceClient _ordersClient;
    private readonly ProductService.ProductServiceClient _productsClient;
    private readonly IDistributedCache _cache;
    private readonly ILogger<ProfileAggregator> _logger;

    public ProfileAggregator(
        InfoService.InfoServiceClient infoClient,
        OrdersService.Protos.OrdersService.OrdersServiceClient ordersClient,
        ProductService.ProductServiceClient productsClient,
        IDistributedCache cache,
        ILogger<ProfileAggregator> logger)
    {
        _infoClient = infoClient;
        _ordersClient = ordersClient;
        _productsClient = productsClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<UserProfile?> GetProfileAsync(string userId, CancellationToken cancellationToken)
    {
        var cacheKey = $"profile:{userId}";
        var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (cached != null)
        {
            return JsonSerializer.Deserialize<UserProfile>(cached);
        }

        UserResponse? user = null;
        OrdersResponse? orders = null;
        List<ProductResponse> products = new();

        try
        {
            user = await _infoClient.GetUserAsync(new GetUserRequest { UserId = userId }, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch user {UserId}", userId);
            throw;
        }

        try
        {
            orders = await _ordersClient.GetUserOrdersAsync(new GetUserOrdersRequest { UserId = userId, Limit = 5, Offset = 0 }, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch orders for user {UserId}", userId);
            orders = new OrdersResponse();
        }

        if (orders?.Orders != null)
        {
            var productIds = orders.Orders.SelectMany(o => o.Items).Select(i => i.ProductId).Distinct().Take(5).ToList();
            var productTasks = productIds.Select(async pid =>
            {
                try
                {
                    return await _productsClient.GetProductAsync(new GetProductRequest { Id = pid }, cancellationToken: cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch product {ProductId}", pid);
                    return null;
                }
            });

            var productResults = await Task.WhenAll(productTasks);
            products.AddRange(productResults.Where(p => p != null)!);
        }

        var result = new UserProfile
        {
            User = user,
            RecentOrders = orders?.Orders,
            FeaturedProducts = products
        };

        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        }, cancellationToken);

        return result;
    }
}

