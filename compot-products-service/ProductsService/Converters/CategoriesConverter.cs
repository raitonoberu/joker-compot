using ProductsService.Data.Entities;
using ProductsService.Protos;

namespace ProductsService.Converters;

public static class CategoriesConverter
{
    public static CategoryResponse ToProtoResponse(this Category category)
    {
        return new CategoryResponse
        {
            Id = category.Id.ToString(),
            Name = category.Name,
            Slug = category.Slug
        };
    }
}