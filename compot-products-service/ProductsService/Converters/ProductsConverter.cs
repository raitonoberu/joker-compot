using ProductsService.Data.Entities;

namespace ProductsService.Converters;

public static class ProductsConverter
{
    public static Protos.ProductResponse ToProtoResponse(this Product product)
    {
        return new Protos.ProductResponse
        {
            Id = product.Id.ToString(),
            Category = product.Category.ToProtoResponse(),
            Description = product.Description,
            Name = product.Name,
            Price = product.Price,
            StockQuantity = product.StockQuantity
        };
    }
}