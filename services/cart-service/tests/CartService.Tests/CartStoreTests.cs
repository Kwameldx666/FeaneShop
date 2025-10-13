using CartService.Models;

namespace CartService.Tests;

public class CartStoreTests
{
    [Fact]
    public void AddingItemCreatesCart()
    {
        var store = new InMemoryCartStore();
        var userId = Guid.NewGuid();
        var item = store.AddItem(userId, new AddCartItemRequest(Guid.NewGuid(), 2));

        Assert.Equal(userId, store.GetCart(userId).UserId);
        Assert.Equal(2, item.Quantity);
    }
}
