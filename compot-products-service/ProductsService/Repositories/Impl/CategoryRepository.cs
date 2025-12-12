using Microsoft.EntityFrameworkCore;
using ProductsService.Data;
using ProductsService.Data.Entities;

namespace ProductsService.Repositories.Impl;

public sealed class CategoryRepository(ProductsContext context) : ICategoryRepository
{
    public async Task<Category> CreateAsync(Category category, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(category);
        await context.AddAsync(category, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return category;
    }

    public Task<Category?> GetAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return context
            .Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == categoryId, cancellationToken);
    }

    public async Task<Category> DeleteAsync(Category category, CancellationToken cancellationToken = default)
    {
        context.Categories.Remove(category);
        await context.SaveChangesAsync(cancellationToken);

        return category;
    }

    public Task<List<Category>> GetAsync(int offset, int limit, string? text, CancellationToken cancellationToken = default)
    {
        var lowerText = text?.ToLower();
        return context
            .Categories
            .AsNoTracking()
            .Where(x => string.IsNullOrEmpty(lowerText)
                        || x.Name.ToLower().Contains(lowerText)
                        || x.Slug.ToLower().Contains(lowerText)) //todo: better use tsvector
            .OrderByDescending(x => x.Name)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<Category> UpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        context.Categories.Update(category);
        await context.SaveChangesAsync(cancellationToken);
        return category;
    }
}