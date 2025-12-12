using ProductsService.Data.Entities;

namespace ProductsService.Repositories;

public interface IProductsRepository
{
    Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default);

    Task<Product?> GetAsync(Guid productId, CancellationToken cancellationToken = default);

    Task<List<Product>> GetAsync(
        int offset,
        int limit,
        string? text,
        Guid? categoryId,
        CancellationToken cancellationToken = default);

    Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken = default);

    Task<Product> DeleteAsync(Product product, CancellationToken cancellationToken = default);
}