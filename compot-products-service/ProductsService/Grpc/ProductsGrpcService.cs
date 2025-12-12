using Grpc.Core;
using ProductsService.Converters;
using ProductsService.Data.Entities;
using ProductsService.Protos;
using ProductsService.Repositories;

namespace ProductsService.Grpc;

public sealed class ProductsGrpcService(
    IProductsRepository productsRepository,
    ILogger<ProductsGrpcService> logger)
    : ProductService.ProductServiceBase
{
    public override async Task<ProductResponse> CreateProduct(CreateProductRequest request, ServerCallContext context)
    {
        try
        {
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                CategoryId = Guid.Parse(request.CategoryId),
                Description = request.Description,
                Name = request.Name,
                Price = request.Price,
                StockQuantity = request.StockQuantity
            };
            var entity = await productsRepository.CreateAsync(product, context.CancellationToken);

            return entity.ToProtoResponse();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error product create");
            throw new RpcException(new Status(StatusCode.Internal, "Internal Server Error"));
        }
    }

    public override async Task<ProductResponse> DeleteProduct(DeleteProductRequest request, ServerCallContext context)
    {
        try
        {
            var product = await productsRepository.GetAsync(Guid.Parse(request.Id), context.CancellationToken)
                ?? throw new RpcException(new Status(StatusCode.NotFound, $"Product with id [{request.Id}] not found"));
            var result = await productsRepository.DeleteAsync(product, context.CancellationToken);

            return result.ToProtoResponse();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error product delete");
            throw new RpcException(new Status(StatusCode.Internal, "Internal Server Error"));
        }
    }

    public override async Task<ProductResponse> GetProduct(GetProductRequest request, ServerCallContext context)
    {
        try
        {
            var product = await productsRepository.GetAsync(Guid.Parse(request.Id), context.CancellationToken)
                ?? throw new RpcException(new Status(StatusCode.NotFound, $"Product with id [{request.Id}] not found"));

            return product.ToProtoResponse();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error product get");
            throw new RpcException(new Status(StatusCode.Internal, "Internal Server Error"));
        }
    }

    public override async Task<ListProductsResponse> ListProducts(ListProductsRequest request, ServerCallContext context)
    {
        try
        {
            var product = await productsRepository.GetAsync(
                request.Offset,
                request.Limit,
                request.Text,
                request.HasCategoryId ? Guid.Parse(request.CategoryId) : null,
                context.CancellationToken);
            var response = new ListProductsResponse();
            response.Products.AddRange(product.Select(x => x.ToProtoResponse()));

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error product list");
            throw new RpcException(new Status(StatusCode.Internal, "Internal Server Error"));
        }
    }

    public override async Task<ProductResponse> UpdateProduct(UpdateProductRequest request, ServerCallContext context)
    {
        
        try
        {
            var product = await productsRepository.GetAsync(Guid.Parse(request.Id), context.CancellationToken)
                          ?? throw new RpcException(new Status(StatusCode.NotFound, $"Product with id [{request.Id}] not found"));

            product.Name = request.Name;
            product.Description = request.Description;
            product.CategoryId = Guid.Parse(request.CategoryId);
            product.Category = null;
            product.Price = request.Price;
            product.StockQuantity = request.StockQuantity;
            var result = await productsRepository.UpdateAsync(product, context.CancellationToken);

            return result.ToProtoResponse();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error product update");
            throw new RpcException(new Status(StatusCode.Internal, "Internal Server Error"));
        }
    }
}