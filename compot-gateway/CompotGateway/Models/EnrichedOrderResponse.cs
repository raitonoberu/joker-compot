using OrdersService.Protos;

namespace CompotGateway.Models;

public class EnrichedOrderResponse
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public List<EnrichedOrderItem> Items { get; set; } = [];
    public double TotalPrice { get; set; }
    public OrderStatus Status { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}

public class EnrichedOrderItem
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public double Price { get; set; }
    public string? ProductName { get; set; }
    public string? ProductDescription { get; set; }
}

public class EnrichedOrdersListResponse
{
    public List<EnrichedOrderResponse> Orders { get; set; } = [];
}

