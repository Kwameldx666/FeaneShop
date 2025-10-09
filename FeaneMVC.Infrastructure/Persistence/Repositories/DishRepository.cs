using FeaneMVC.Application.Common.Interfaces.Persistence;
using FeaneMVC.Domain.Entities;
using FeaneMVC.Domain.ValueObjects;
using FeaneMVC.Infrastructure.Persistence.Db;
using Microsoft.EntityFrameworkCore;

namespace FeaneMVC.Infrastructure.Persistence.Repositories
{
    public class DishRepository : IDishReadRepository, IDishWriteRepository
    {
        private static readonly string[] Categories = new[] { "Breakfast", "Lunch", "Dinner", "Dessert", "Drinks", "Snacks" };
        private static readonly string[] Adjectives = new[] { "Spicy", "Sweet", "Savory", "Crispy", "Creamy", "Zesty", "Smoky", "Herbed", "Golden", "Fresh" };
        private static readonly string[] Nouns = new[] { "Delight", "Fusion", "Bowl", "Platter", "Bite", "Skillet", "Feast", "Medley", "Stack", "Treat" };
        private static readonly string[] Ingredients = new[] { "avocado", "grilled chicken", "roasted peppers", "fresh herbs", "garlic aioli", "parmesan", "wild mushrooms", "citrus glaze", "balsamic reduction", "truffle oil" };
        private static readonly string[] ImagePool = new[] { "f2.png", "f3.png", "f4.png", "f5.png", "f6.png", "f7.png", "f8.png", "f9.png", "o1.jpg", "o2.jpg" };

        private readonly ApplicationDbContext _context;
        private readonly Random _random = new();

        public DishRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Dish>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Dishes
                    .AsNoTracking()
                    .OrderBy(dish => dish.Name)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching all dishes: {ex.Message}");
                return Array.Empty<Dish>();
            }
        }

        public async Task<Dish?> GetByIdAsync(Guid dishId, CancellationToken cancellationToken = default)
        {
            if (dishId == Guid.Empty)
            {
                return null;
            }

            try
            {
                return await _context.Dishes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(dish => dish.Id == dishId, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching dish by ID: {ex.Message}");
                return null;
            }
        }

        public async Task<OperationResult<Dish>> AddAsync(Dish dish, CancellationToken cancellationToken = default)
        {
            if (dish == null)
            {
                return OperationResult<Dish>.Failure("Dish cannot be null");
            }

            try
            {
                if (dish.Id == Guid.Empty)
                {
                    dish.Id = Guid.NewGuid();
                }

                dish.CreatedAt = DateTime.UtcNow;
                dish.UpdatedAt = dish.CreatedAt;

                await _context.Dishes.AddAsync(dish, cancellationToken);

                return OperationResult<Dish>.Success(dish, "Dish added successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding dish: {ex.Message}");
                return OperationResult<Dish>.Failure("Error adding dish");
            }
        }

        public async Task<OperationResult<Dish>> UpdateAsync(Guid dishId, Dish dish, CancellationToken cancellationToken = default)
        {
            if (dishId == Guid.Empty || dish == null)
            {
                return OperationResult<Dish>.Failure("Invalid input");
            }

            try
            {
                var existingDish = await _context.Dishes.FirstOrDefaultAsync(entity => entity.Id == dishId, cancellationToken);
                if (existingDish == null)
                {
                    return OperationResult<Dish>.Failure("Dish not found");
                }

                existingDish.Name = dish.Name;
                existingDish.Description = dish.Description;
                existingDish.Price = dish.Price;
                existingDish.Category = dish.Category;
                existingDish.ImageUrl = dish.ImageUrl;
                existingDish.UpdatedAt = DateTime.UtcNow;

                return OperationResult<Dish>.Success(existingDish, "Dish updated successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating dish: {ex.Message}");
                return OperationResult<Dish>.Failure("Error updating dish");
            }
        }

        public async Task<OperationResult> DeleteAsync(Guid dishId, CancellationToken cancellationToken = default)
        {
            if (dishId == Guid.Empty)
            {
                return OperationResult.Failure("Invalid dish ID");
            }

            try
            {
                var dish = await _context.Dishes.FirstOrDefaultAsync(entity => entity.Id == dishId, cancellationToken);
                if (dish == null)
                {
                    return OperationResult.Failure("Dish not found");
                }

                _context.Dishes.Remove(dish);

                return OperationResult.Success("Dish deleted successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting dish: {ex.Message}");
                return OperationResult.Failure("Error deleting dish");
            }
        }

        public async Task<OperationResult<BulkSeedSummary>> SeedRandomAsync(int count, CancellationToken cancellationToken = default)
        {
            if (count <= 0)
            {
                return OperationResult<BulkSeedSummary>.Failure("Count must be greater than zero");
            }

            const int maxCount = 200;
            var targetCount = Math.Min(count, maxCount);

            try
            {
                var existingNames = await _context.Dishes
                    .AsNoTracking()
                    .Select(d => d.Name)
                    .ToListAsync(cancellationToken);

                var uniqueNames = new HashSet<string>(existingNames, StringComparer.OrdinalIgnoreCase);
                var dishesToAdd = new List<Dish>();
                var skipped = 0;
                var attempts = 0;
                var maxAttempts = targetCount * 5;

                while (dishesToAdd.Count < targetCount && attempts < maxAttempts)
                {
                    attempts++;
                    var name = GenerateName();

                    if (!uniqueNames.Add(name))
                    {
                        skipped++;
                        continue;
                    }

                    dishesToAdd.Add(new Dish
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        Description = GenerateDescription(),
                        Price = GeneratePrice(),
                        Category = Categories[_random.Next(Categories.Length)],
                        ImageUrl = $"/images/{ImagePool[_random.Next(ImagePool.Length)]}",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                if (dishesToAdd.Count == 0)
                {
                    return OperationResult<BulkSeedSummary>.Failure("No unique dishes could be generated.");
                }

                await _context.Dishes.AddRangeAsync(dishesToAdd, cancellationToken);

                var summary = new BulkSeedSummary
                {
                    CreatedCount = dishesToAdd.Count,
                    SkippedCount = skipped
                };

                return OperationResult<BulkSeedSummary>.Success(summary, "Sample dishes generated successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding dishes: {ex.Message}");
                return OperationResult<BulkSeedSummary>.Failure("Error generating sample dishes");
            }
        }

        private string GenerateName()
        {
            var adjective = Adjectives[_random.Next(Adjectives.Length)];
            var noun = Nouns[_random.Next(Nouns.Length)];
            return $"{adjective} {noun}";
        }

        private string GenerateDescription()
        {
            var ingredientSample = Ingredients
                .OrderBy(_ => _random.Next())
                .Take(3)
                .ToArray();

            if (ingredientSample.Length == 0)
            {
                return "Chef's special creation.";
            }

            if (ingredientSample.Length == 1)
            {
                return $"A bold serving of {ingredientSample[0]}.";
            }

            return $"A {ingredientSample[0]} with {string.Join(", ", ingredientSample.Skip(1))}.";
        }

        private decimal GeneratePrice()
        {
            var price = _random.NextDouble() * 20 + 5; // Between 5 and 25
            return Math.Round((decimal)price, 2);
        }
    }
}
