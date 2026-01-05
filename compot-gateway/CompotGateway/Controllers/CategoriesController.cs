using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductsService.Protos;

namespace CompotGateway.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(CategoryService.CategoryServiceClient categoryClient) : ControllerBase
{
    [HttpGet("{id}")]
    [ProducesResponseType<CategoryResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CategoryResponse>> Get(string id, CancellationToken cancellationToken)
    {
        var response = await categoryClient.GetCategoryAsync(new GetCategoryRequest { Id = id }, cancellationToken: cancellationToken);
        return Ok(response);
    }

    [HttpGet]
    [ProducesResponseType<ListCategoriesResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ListCategoriesResponse>> List(
        [FromQuery] int limit = 10,
        [FromQuery] int offset = 0,
        [FromQuery] string? text = null,
        CancellationToken cancellationToken = default)
    {
        var request = new ListCategoriesRequest
        {
            Limit = limit,
            Offset = offset
        };
        if (!string.IsNullOrEmpty(text)) request.Text = text;
        var response = await categoryClient.ListCategoriesAsync(request, cancellationToken: cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType<CategoryResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<CategoryResponse>> Create([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var response = await categoryClient.CreateCategoryAsync(request, cancellationToken: cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
    }

    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType<CategoryResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CategoryResponse>> Update(string id, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
        {
            return BadRequest("ID mismatch");
        }

        var response = await categoryClient.UpdateCategoryAsync(request, cancellationToken: cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType<CategoryResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CategoryResponse>> Delete(string id, CancellationToken cancellationToken)
    {
        var response = await categoryClient.DeleteCategoryAsync(new DeleteCategoryRequest { Id = id }, cancellationToken: cancellationToken);
        return Ok(response);
    }
}

