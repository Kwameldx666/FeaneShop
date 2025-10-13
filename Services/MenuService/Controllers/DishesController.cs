using System.Linq;
using Feane.Contracts.Dishes;
using FeaneMVC.Application.Commands.Dishes;
using FeaneMVC.Application.Queries.Dishes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Feane.MenuService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DishesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DishesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DishResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DishResponse>>> GetAllAsync()
    {
        var dishes = await _mediator.Send(new GetAllDishesQuery());
        return Ok(dishes.Select(dto => new DishResponse
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Category = dto.Category,
            ImageUrl = dto.ImageUrl,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        }));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DishResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DishResponse>> GetByIdAsync(Guid id)
    {
        var result = await _mediator.Send(new GetDishByIdQuery(id));
        if (result?.Data is null || !result.Status)
        {
            return NotFound();
        }

        var dish = result.Data;
        return Ok(new DishResponse
        {
            Id = dish.Id,
            Name = dish.Name,
            Description = dish.Description,
            Price = dish.Price,
            Category = dish.Category,
            ImageUrl = dish.ImageUrl,
            CreatedAt = dish.CreatedAt,
            UpdatedAt = dish.UpdatedAt
        });
    }

    [HttpPost]
    [ProducesResponseType(typeof(DishResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DishResponse>> CreateAsync([FromBody] CreateDishRequest request)
    {
        var response = await _mediator.Send(new CreateDishCommand(
            request.Name,
            request.Description,
            request.Price,
            request.Category,
            request.ImageUrl));

        if (!response.Status || response.Data is null)
        {
            return BadRequest(response.Message ?? "Failed to create dish");
        }

        var createdDish = response.Data;
        var result = new DishResponse
        {
            Id = createdDish.Id,
            Name = createdDish.Name,
            Description = createdDish.Description,
            Price = createdDish.Price,
            Category = createdDish.Category,
            ImageUrl = createdDish.ImageUrl,
            CreatedAt = createdDish.CreatedAt,
            UpdatedAt = createdDish.UpdatedAt
        };

        return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateDishRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest("Mismatched dish identifier");
        }

        var response = await _mediator.Send(new UpdateDishCommand(
            request.Id,
            request.Name,
            request.Description,
            request.Price,
            request.Category,
            request.ImageUrl));

        if (!response.Status)
        {
            return BadRequest(response.Message ?? "Failed to update dish");
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        await _mediator.Send(new DeleteDishCommand(id));
        return NoContent();
    }
}
