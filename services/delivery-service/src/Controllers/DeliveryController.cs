using Microsoft.AspNetCore.Mvc;
using DeliveryService.Models;

namespace DeliveryService.Controllers;

[ApiController]
[Route("api/delivery")]
public class DeliveryController(IDeliveryTracker tracker) : ControllerBase
{
    [HttpPost]
    public ActionResult<DeliveryStatusRecord> Create([FromBody] CreateDeliveryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Address))
        {
            ModelState.AddModelError(nameof(request.Address), "Address is required.");
            return ValidationProblem(ModelState);
        }

        var record = tracker.Create(request.OrderId, request.Address);
        return CreatedAtAction(nameof(Get), new { orderId = record.OrderId }, record);
    }

    [HttpGet("{orderId:guid}")]
    public ActionResult<DeliveryStatusRecord> Get(Guid orderId)
        => tracker.Get(orderId) is { } record ? Ok(record) : NotFound();

    [HttpPost("{orderId:guid}/stage")]
    public ActionResult<DeliveryStatusRecord> Update(Guid orderId, [FromBody] DeliveryStage stage)
        => Ok(tracker.Update(orderId, stage));
}

public record CreateDeliveryRequest(Guid OrderId, string Address);
