using Microsoft.AspNetCore.Mvc;
using OrderService.Models;

namespace OrderService.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController(IOrderStore store) : ControllerBase
{
    [HttpPost]
    public ActionResult<Order> CreateOrder([FromBody] CreateOrderRequest request)
    {
        if (request.Items.Count == 0)
        {
            ModelState.AddModelError(nameof(request.Items), "At least one item must be provided.");
            return ValidationProblem(ModelState);
        }

        var order = store.Create(request);
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    [HttpGet("{id:guid}")]
    public ActionResult<Order> GetOrder(Guid id)
        => store.GetById(id) is { } order ? Ok(order) : NotFound();

    [HttpGet("user/{userId:guid}")]
    public ActionResult<IEnumerable<Order>> GetForUser(Guid userId) => Ok(store.GetForUser(userId));

    [HttpPost("{id:guid}/status")]
    public ActionResult<Order> UpdateStatus(Guid id, [FromBody] OrderStatus status)
        => Ok(store.UpdateStatus(id, status));
}
