using CompotGateway.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdersService.Protos;
using ProductsService.Protos;

namespace CompotGateway.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrdersService.Protos.OrdersService.OrdersServiceClient _ordersClient;
    private readonly ProductService.ProductServiceClient _productsClient;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        OrdersService.Protos.OrdersService.OrdersServiceClient ordersClient,
        ProductService.ProductServiceClient productsClient,
        ILogger<OrdersController> logger)
    {
        _ordersClient = ordersClient;
        _productsClient = productsClient;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var response = await _ordersClient.CreateOrderAsync(request, cancellationToken: cancellationToken);
        return CreatedAtAction(nameof(GetOrder), new { id = response.Id }, response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<EnrichedOrderResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<EnrichedOrderResponse>> GetOrder(string id, CancellationToken cancellationToken)
    {
        var order = await _ordersClient.GetOrderAsync(new GetOrderRequest { OrderId = id }, cancellationToken: cancellationToken);
        var enrichedOrder = await EnrichOrderWithProducts(order, cancellationToken);
        return Ok(enrichedOrder);
    }

    [HttpGet("user/{userId}")]
    [ProducesResponseType<EnrichedOrdersListResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<EnrichedOrdersListResponse>> GetUserOrders(string userId, [FromQuery] int limit = 10, [FromQuery] int offset = 0, CancellationToken cancellationToken = default)
    {
        var ordersResponse = await _ordersClient.GetUserOrdersAsync(new GetUserOrdersRequest { UserId = userId, Limit = limit, Offset = offset }, cancellationToken: cancellationToken);

        var enrichedOrders = new List<EnrichedOrderResponse>();
        foreach (var order in ordersResponse.Orders)
        {
            enrichedOrders.Add(await EnrichOrderWithProducts(order, cancellationToken));
        }

        return Ok(new EnrichedOrdersListResponse { Orders = enrichedOrders });
    }

    [HttpPut("{id}/status")]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<OrderResponse>> UpdateOrderStatus(string id, [FromBody] UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        if (id != request.OrderId)
        {
            return BadRequest("ID mismatch");
        }

        var response = await _ordersClient.UpdateOrderStatusAsync(request, cancellationToken: cancellationToken);
        return Ok(response);
    }

    private async Task<EnrichedOrderResponse> EnrichOrderWithProducts(OrderResponse order, CancellationToken cancellationToken)
    {
        var enrichedItems = new List<EnrichedOrderItem>();
        foreach (var item in order.Items)
        {
            ProductResponse? product = null;
            try
            {
                product = await _productsClient.GetProductAsync(new GetProductRequest { Id = item.ProductId }, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch product {ProductId}", item.ProductId);
            }

            enrichedItems.Add(new EnrichedOrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Price,
                ProductName = product?.Name,
                ProductDescription = product?.Description
            });
        }

        return new EnrichedOrderResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            Items = enrichedItems,
            TotalPrice = order.TotalPrice,
            Status = order.Status,
            CreatedAt = order.CreatedAt
        };
    }
}

