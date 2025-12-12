namespace ProductsService.Data.Entities;

public sealed class Category
{
    public required Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}