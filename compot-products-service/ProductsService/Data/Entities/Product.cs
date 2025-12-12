namespace ProductsService.Data.Entities;

public sealed class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Price { get; set; }
    public Guid CategoryId { get; set; }
    public int StockQuantity { get; set; }

    public Category Category { get; set; } = null!;
}