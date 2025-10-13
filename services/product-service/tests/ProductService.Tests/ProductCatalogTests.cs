using ProductService.Models;

namespace ProductService.Tests;

public class ProductCatalogTests
{
    [Fact]
    public void CatalogStartsWithSeedItem()
    {
        var catalog = new InMemoryProductCatalog();
        Assert.NotEmpty(catalog.GetAll());
    }
}
