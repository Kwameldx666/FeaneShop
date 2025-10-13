namespace CatalogService.Api;

public class MenuCatalog
{
    private readonly List<MenuItem> _items =
    [
        new(1, "Margherita Pizza", "Пиццы", 12.5m, "Классическая тонкая пицца с томатами и базиликом"),
        new(2, "Truffle Pasta", "Паста", 15.9m, "Паста с белыми грибами и трюфельным маслом"),
        new(3, "Caesar Salad", "Салаты", 8.9m, "Хрустящий салат с курицей и пармезаном"),
        new(4, "Tom Yum", "Супы", 11.5m, "Пряный тайский суп с креветками"),
        new(5, "Berry Cheesecake", "Десерты", 6.5m, "Воздушный чизкейк с ягодным конфитюром"),
        new(6, "Matcha Latte", "Напитки", 4.2m, "Матча на кокосовом молоке"),
        new(7, "Vegan Bowl", "Боулы", 10.4m, "Боул с киноа, авокадо и нутом"),
        new(8, "BBQ Burger", "Бургеры", 13.2m, "Сочный бургер с копчёным соусом BBQ"),
        new(9, "Ramen", "Супы", 12.1m, "Японский рамен с свининой и яйцом"),
        new(10, "Tiramisu", "Десерты", 6.9m, "Классический десерт с маскарпоне"),
    ];

    public IReadOnlyCollection<MenuItem> GetMenu() => _items;
}

public record MenuItem(int Id, string Name, string Category, decimal Price, string Description);
