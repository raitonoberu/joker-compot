using ProductsService.Data.Entities;

namespace ProductsService.Repositories;

public interface ICategoryRepository
{
    Task<Category> CreateAsync(Category category, CancellationToken cancellationToken = default);

    Task<Category?> GetAsync(Guid categoryId, CancellationToken cancellationToken = default);

    Task<Category> UpdateAsync(Category category, CancellationToken cancellationToken = default);

    Task<Category> DeleteAsync(Category category, CancellationToken cancellationToken = default);

    Task<List<Category>> GetAsync(
        int offset,
        int limit,
        string? text,
        CancellationToken cancellationToken = default);
}