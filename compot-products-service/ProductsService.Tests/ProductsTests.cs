using Grpc.Net.Client;
using NUnit.Framework;
using ProductsService.Protos;
using CategoryClient = ProductsService.Protos.CategoryService.CategoryServiceClient;
using ProductsClient = ProductsService.Protos.ProductService.ProductServiceClient;

namespace ProductsService.Tests;

public class ProductsTests
{
    private static readonly GrpcChannel Channel = GrpcChannel.ForAddress("http://localhost:7000");
    private readonly ProductsClient _productsClient = new(Channel);
    private readonly CategoryClient _categoryClient = new(Channel);

    [Test]
    public void CreateProduct_ThenGet()
    {
        var name = Guid.NewGuid().ToString();
        var slug = Guid.NewGuid().ToString();
        var request = new CreateCategoryRequest
        {
            Name = name,
            Slug = slug
        };
        var category = _categoryClient.CreateCategory(request);

        var createProductRequest = GetCreateProductRequest(category.Id);
        var response = _productsClient.CreateProduct(createProductRequest);

        var product = _productsClient.GetProduct(new GetProductRequest
        {
           Id = response.Id
        });

        Assert.That(product.Category.Id, Is.EqualTo(category.Id));
        Assert.That(product.Name, Is.EqualTo(createProductRequest.Name));
        Assert.That(product.Description, Is.EqualTo(createProductRequest.Description));
        Assert.That(product.StockQuantity, Is.EqualTo(createProductRequest.StockQuantity));
        Assert.That(product.Price, Is.EqualTo(createProductRequest.Price));

        _productsClient.DeleteProduct(new DeleteProductRequest
        {
            Id = product.Id
        });
        _categoryClient.DeleteCategory(new DeleteCategoryRequest
        {
            Id = category.Id
        });
    }

    [Test]
    public void CreateProduct_ThenUpdate()
    {
        var createCategory = new CreateCategoryRequest
        {
            Name = Guid.NewGuid().ToString(),
            Slug = Guid.NewGuid().ToString()
        };
        var category = _categoryClient.CreateCategory(createCategory);

        var createProductRequest = GetCreateProductRequest(category.Id);
        var response = _productsClient.CreateProduct(createProductRequest);

        var product = _productsClient.GetProduct(new GetProductRequest
        {
            Id = response.Id
        });

        Assert.That(product.Category.Id, Is.EqualTo(category.Id));
        Assert.That(product.Name, Is.EqualTo(createProductRequest.Name));
        Assert.That(product.Description, Is.EqualTo(createProductRequest.Description));
        Assert.That(product.StockQuantity, Is.EqualTo(createProductRequest.StockQuantity));
        Assert.That(product.Price, Is.EqualTo(createProductRequest.Price));

        var createCategory2 = new CreateCategoryRequest
        {
            Name = Guid.NewGuid().ToString(),
            Slug = Guid.NewGuid().ToString()
        };
        var category2 = _categoryClient.CreateCategory(createCategory2);

        var updateRequest = new UpdateProductRequest
        {
            Id = product.Id,
            Description = "dat31tfas",
            CategoryId = category2.Id,
            Name = "test-name",
            Price = 5115,
            StockQuantity = 41
        };

        _productsClient.UpdateProduct(updateRequest);
        product = _productsClient.GetProduct(new GetProductRequest
        {
            Id = response.Id
        });

        Assert.That(product.Id, Is.EqualTo(updateRequest.Id));
        Assert.That(product.Name, Is.EqualTo(updateRequest.Name));
        Assert.That(product.Description, Is.EqualTo(updateRequest.Description));
        Assert.That(product.StockQuantity, Is.EqualTo(updateRequest.StockQuantity));
        Assert.That(product.Price, Is.EqualTo(updateRequest.Price));
        Assert.That(product.Category.Id, Is.EqualTo(updateRequest.CategoryId));

        _productsClient.DeleteProduct(new DeleteProductRequest
        {
            Id = product.Id
        });
        _categoryClient.DeleteCategory(new DeleteCategoryRequest
        {
            Id = category.Id
        });
        _categoryClient.DeleteCategory(new DeleteCategoryRequest
        {
            Id = category2.Id
        });
    }

    [Test]
    public void CreateProducts_ThenQuery()
    {
        var name = Guid.NewGuid().ToString();
        var slug = Guid.NewGuid().ToString();
        var request = new CreateCategoryRequest
        {
            Name = name,
            Slug = slug
        };
        var category = _categoryClient.CreateCategory(request);

        for (var i = 0; i < 8; i++)
        {
            var createProductRequest = GetCreateProductRequest(category.Id);
            _productsClient.CreateProduct(createProductRequest);
        }

        var products = _productsClient.ListProducts(new ListProductsRequest
        {
            Offset = 0,
            Limit = 100,
            CategoryId = category.Id
        });

        Assert.That(products.Products.Count, Is.EqualTo(8));
        Assert.That(products.Products.Select(x => x.Category.Id), Is.All.EqualTo(category.Id));

        var listOneProductRequest = new ListProductsRequest
        {
            Offset = 0,
            Limit = 100,
            CategoryId = category.Id,
            Text = products.Products.First().Description
        };
        var product = _productsClient.ListProducts(listOneProductRequest);
        
        Assert.That(product.Products.Count, Is.EqualTo(1));
        Assert.That(product.Products[0].Description, Is.EqualTo(listOneProductRequest.Text));

        foreach (var p in products.Products)
        {
            _productsClient.DeleteProduct(new DeleteProductRequest
            {
                Id = p.Id
            });
        }
        _categoryClient.DeleteCategory(new DeleteCategoryRequest
        {
            Id = category.Id
        });
    }

    private CreateProductRequest GetCreateProductRequest(string categoryId)
    {
        return new CreateProductRequest
        {
            CategoryId = categoryId,
            Description = Guid.NewGuid().ToString(),
            Name = Guid.NewGuid().ToString(),
            Price = 41341,
            StockQuantity = 52
        };
    }
}