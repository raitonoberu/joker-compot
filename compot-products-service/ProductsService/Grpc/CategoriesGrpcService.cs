using Grpc.Core;
using ProductsService.Converters;
using ProductsService.Data.Entities;
using ProductsService.Protos;
using ProductsService.Repositories;

namespace ProductsService.Grpc;

public class CategoriesGrpcService(
    ICategoryRepository categoryRepository,
    ILogger<ProductsGrpcService> logger)
    : CategoryService.CategoryServiceBase
{
    public override async Task<CategoryResponse> CreateCategory(CreateCategoryRequest request, ServerCallContext context)
    {
        try
        {
            var categoryId = Guid.NewGuid();
            var category = new Category
            {
                Id = categoryId,
                Name = request.Name,
                Slug = request.Slug
            };
            await categoryRepository.CreateAsync(category, context.CancellationToken);

            return category.ToProtoResponse();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error category create");
            throw new RpcException(new Status(StatusCode.Internal, "Internal Server Error"));
        }
    }

    public override async Task<CategoryResponse> DeleteCategory(DeleteCategoryRequest request, ServerCallContext context)
    {
        try
        {
            var category = await categoryRepository.GetAsync(Guid.Parse(request.Id), context.CancellationToken)
                          ?? throw new RpcException(new Status(StatusCode.NotFound, $"Category with id [{request.Id}] not found"));
            var result = await categoryRepository.DeleteAsync(category, context.CancellationToken);

            return result.ToProtoResponse();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error category delete");
            throw new RpcException(new Status(StatusCode.Internal, "Internal Server Error"));
        }
    }

    public override async Task<CategoryResponse> GetCategory(GetCategoryRequest request, ServerCallContext context)
    {
        try
        {
            var category = await categoryRepository.GetAsync(Guid.Parse(request.Id), context.CancellationToken)
                           ?? throw new RpcException(new Status(StatusCode.NotFound, $"Category with id [{request.Id}] not found"));

            return category.ToProtoResponse();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error category get");
            throw new RpcException(new Status(StatusCode.Internal, "Internal Server Error"));
        }
    }

    public override async Task<ListCategoriesResponse> ListCategories(ListCategoriesRequest request, ServerCallContext context)
    {
        try
        {
            var categories = await categoryRepository.GetAsync(
                request.Offset,
                request.Limit,
                request.Text,
                context.CancellationToken);
            var response = new ListCategoriesResponse();
            response.Categories.AddRange(categories.Select(x => x.ToProtoResponse()));

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error category get");
            throw new RpcException(new Status(StatusCode.Internal, "Internal Server Error"));
        }
    }

    public override async Task<CategoryResponse> UpdateCategory(UpdateCategoryRequest request, ServerCallContext context)
    {
        try
        {
            var category = await categoryRepository.GetAsync(Guid.Parse(request.Id), context.CancellationToken)
                           ?? throw new RpcException(new Status(StatusCode.NotFound, $"Category with id [{request.Id}] not found"));

            category.Name = request.Name;
            category.Slug = request.Slug;
            await categoryRepository.UpdateAsync(category, context.CancellationToken);

            return category.ToProtoResponse();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error category update");
            throw new RpcException(new Status(StatusCode.Internal, "Internal Server Error"));
        }
    }
}