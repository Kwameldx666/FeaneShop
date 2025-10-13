using Microsoft.AspNetCore.Mvc;
using CartService.Models;

namespace CartService.Controllers;

[ApiController]
[Route("api/cart/{userId:guid}")]
public class CartController(ICartStore store) : ControllerBase
{
    [HttpGet]
    public ActionResult<Cart> GetCart(Guid userId) => Ok(store.GetCart(userId));

    [HttpPost("items")]
    public ActionResult<CartItem> AddItem(Guid userId, [FromBody] AddCartItemRequest request)
    {
        if (request.Quantity <= 0)
        {
            ModelState.AddModelError(nameof(request.Quantity), "Quantity must be greater than zero.");
            return ValidationProblem(ModelState);
        }

        var created = store.AddItem(userId, request);
        return CreatedAtAction(nameof(GetCart), new { userId }, created);
    }

    [HttpDelete("items/{itemId:guid}")]
    public IActionResult RemoveItem(Guid userId, Guid itemId)
    {
        store.RemoveItem(userId, itemId);
        return NoContent();
    }
}
