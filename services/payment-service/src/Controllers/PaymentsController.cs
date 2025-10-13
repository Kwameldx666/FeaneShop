using Microsoft.AspNetCore.Mvc;
using PaymentService.Models;

namespace PaymentService.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController(IPaymentProcessor processor) : ControllerBase
{
    [HttpPost]
    public ActionResult<PaymentReceipt> Authorize([FromBody] PaymentRequest request)
    {
        if (request.Amount <= 0)
        {
            ModelState.AddModelError(nameof(request.Amount), "Amount must be positive.");
            return ValidationProblem(ModelState);
        }

        var receipt = processor.Authorize(request);
        return Ok(receipt);
    }
}
