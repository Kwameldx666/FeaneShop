using PaymentService.Models;

namespace PaymentService.Tests;

public class PaymentProcessorTests
{
    [Fact]
    public void PositiveAmountsAreAuthorized()
    {
        var processor = new FakePaymentProcessor();
        var receipt = processor.Authorize(new PaymentRequest(Guid.NewGuid(), 20m, "USD", "manual"));

        Assert.Equal(PaymentStatus.Authorized, receipt.Status);
    }
}
