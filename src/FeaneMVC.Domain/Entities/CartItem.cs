using System.ComponentModel.DataAnnotations;

namespace FeaneMVC.Domain.Entities
{

    public class CartItem
    {
        public Guid CartId { get; set; }
        public Guid DishId { get; set; }
        public Guid UserId { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative integer.")]
        public int Quantity { get; set; }

        [Range(0.0, double.MaxValue, ErrorMessage = "TotalPrice must be a non-negative value.")]
        public decimal TotalPrice { get; set; }

        public string Name { get; set; }
        public decimal Price { get; set; }
        public Cart Cart { get; set; }
        public Dish Dish { get; set; }
        public UserData User { get; set; }

    }

}
