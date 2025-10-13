using Microsoft.AspNetCore.Mvc;
using ProductService.Models;

namespace ProductService.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(IProductCatalog catalog) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Product>> ListProducts() => Ok(catalog.GetAll());

    [HttpGet("{id:guid}")]
    public ActionResult<Product> GetProduct(Guid id)
        => catalog.GetById(id) is { } product ? Ok(product) : NotFound();

    [HttpPost]
    public ActionResult<Product> CreateProduct([FromBody] CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            ModelState.AddModelError(nameof(request.Name), "Name is required.");
            return ValidationProblem(ModelState);
        }

        if (request.Price <= 0)
        {
            ModelState.AddModelError(nameof(request.Price), "Price must be positive.");
            return ValidationProblem(ModelState);
        }

        var created = catalog.Create(request);
        return CreatedAtAction(nameof(GetProduct), new { id = created.Id }, created);
    }
}
