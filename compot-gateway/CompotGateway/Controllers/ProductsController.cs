using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductsService.Protos;

namespace CompotGateway.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly ProductService.ProductServiceClient _productClient;

    public ProductsController(ProductService.ProductServiceClient productClient)
    {
        _productClient = productClient;
    }

    [HttpGet("{id}")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductResponse>> GetProduct(string id, CancellationToken cancellationToken)
    {
        var response = await _productClient.GetProductAsync(new GetProductRequest { Id = id }, cancellationToken: cancellationToken);
        return Ok(response);
    }

    [HttpGet]
    [ProducesResponseType<ListProductsResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ListProductsResponse>> ListProducts([FromQuery] int limit = 10, [FromQuery] int offset = 0, [FromQuery] string? text = null, [FromQuery] string? categoryId = null, CancellationToken cancellationToken = default)
    {
        var request = new ListProductsRequest
        {
            Limit = limit,
            Offset = offset,
            Text = text,
            CategoryId = categoryId
        };
        var response = await _productClient.ListProductsAsync(request, cancellationToken: cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductResponse>> CreateProduct([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var response = await _productClient.CreateProductAsync(request, cancellationToken: cancellationToken);
        return CreatedAtAction(nameof(GetProduct), new { id = response.Id }, response);
    }

    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductResponse>> UpdateProduct(string id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
        {
            return BadRequest("ID mismatch");
        }

        var response = await _productClient.UpdateProductAsync(request, cancellationToken: cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductResponse>> DeleteProduct(string id, CancellationToken cancellationToken)
    {
        var response = await _productClient.DeleteProductAsync(new DeleteProductRequest { Id = id }, cancellationToken: cancellationToken);
        return Ok(response);
    }
}

