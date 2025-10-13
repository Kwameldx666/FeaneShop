using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers;

[ApiController]
[Route("api")]
public class MenuGatewayController(IHttpClientFactory clients) : ControllerBase
{
    [HttpGet("users")]
    public async Task<IActionResult> ProxyUsers(CancellationToken cancellationToken)
    {
        var client = clients.CreateClient("user-service");
        var response = await client.GetAsync("api/users", cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        return Content(payload, response.Content.Headers.ContentType?.ToString() ?? "application/json");
    }

    [HttpGet("products")]
    public async Task<IActionResult> ProxyProducts(CancellationToken cancellationToken)
    {
        var client = clients.CreateClient("product-service");
        var response = await client.GetAsync("api/products", cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        return Content(payload, response.Content.Headers.ContentType?.ToString() ?? "application/json");
    }
}
