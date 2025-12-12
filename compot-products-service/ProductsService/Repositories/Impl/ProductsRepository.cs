using Microsoft.EntityFrameworkCore;
using ProductsService.Data;
using ProductsService.Data.Entities;

namespace ProductsService.Repositories.Impl;

public sealed class ProductsRepository(ProductsContext context) : IProductsRepository
{
    public async Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);
        await context.AddAsync(product, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return (await GetAsync(product.Id, cancellationToken))!;
    }

    public Task<Product?> GetAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return context
            .Products
            .Include(x => x.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == productId, cancellationToken);
    }

    public Task<List<Product>> GetAsync(
        int offset,
        int limit,
        string? text,
        Guid? categoryId,
        CancellationToken cancellationToken = default)
    {
        var lowerText = text?.ToLower();
        return context
            .Products
            .Include(x => x.Category)
            .AsNoTracking()
            .Where(x => string.IsNullOrEmpty(lowerText)
                        || x.Name.ToLower().Contains(lowerText)
                        || x.Description.ToLower().Contains(lowerText)) //todo: better use tsvector
            .Where(x => categoryId == null || x.CategoryId == categoryId)
            .OrderByDescending(x => x.Name)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        context.Products.Update(product);
        await context.SaveChangesAsync(cancellationToken);
        return (await GetAsync(product.Id, cancellationToken))!;
    }

    public async Task<Product> DeleteAsync(Product product, CancellationToken cancellationToken = default)
    {
        context.Products.Remove(product);
        await context.SaveChangesAsync(cancellationToken);
        return product;
    }
}