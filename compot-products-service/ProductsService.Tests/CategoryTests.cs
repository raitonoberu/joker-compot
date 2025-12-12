using Grpc.Net.Client;
using NUnit.Framework;
using ProductsService.Protos;
using CategoryClient = ProductsService.Protos.CategoryService.CategoryServiceClient;

[assembly: Parallelizable(ParallelScope.All)]

namespace ProductsService.Tests;

public class CategoryTests
{
    private readonly CategoryClient _client = new(GrpcChannel.ForAddress("http://localhost:7000"));

    [Test]
    public void CreateCategory_ThenGet()
    {
        var name = Guid.NewGuid().ToString();
        var slug = Guid.NewGuid().ToString();
        var request = new CreateCategoryRequest
        {
            Name = name,
            Slug = slug
        };
        var response = _client.CreateCategory(request);

        Assert.That(request.Name, Is.EqualTo(response.Name));
        Assert.That(request.Slug, Is.EqualTo(request.Slug));
        Assert.That(response.Id, Is.Not.Default);

        var category = _client.GetCategory(new GetCategoryRequest
        {
            Id = response.Id
        });

        Assert.That(category.Name, Is.EqualTo(name));
        Assert.That(category.Slug, Is.EqualTo(slug));
        Assert.That(category.Id, Is.EqualTo(response.Id));

        _client.DeleteCategory(new DeleteCategoryRequest
        {
            Id = category.Id
        });
    }
}