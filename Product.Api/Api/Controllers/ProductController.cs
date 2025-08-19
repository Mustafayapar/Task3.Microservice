using MassTransit.Mediator;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.Api.Application.Commands;
using Product.Api.Application.Queries;

namespace Product.Api.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class ProductsController : ControllerBase
    {
        private readonly MediatR.IMediator _mediator;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(MediatR.IMediator mediator, ILogger<ProductsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            _logger.LogInformation("Fetching all products");
            var result = await _mediator.Send(new GetProductsQuery(), ct);
            _logger.LogInformation("Fetched {Count} products", result.Count);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            _logger.LogInformation("Fetching product with Id {ProductId}", id);
            var item = await _mediator.Send(new GetProductByIdQuery(id), ct);
            if (item is null)
            {
                _logger.LogWarning("Product with Id {ProductId} not found", id);
                return NotFound();
            }

            _logger.LogInformation("Product {ProductId} retrieved successfully", id);
            return Ok(item);
        }

        [HttpPost] // Ürün Ekleme
        public async Task<IActionResult> Create([FromBody] CreateProductCommand cmd, CancellationToken ct)
        {
            _logger.LogInformation("Creating product with Name {Name}", cmd.Name);
            var id = await _mediator.Send(cmd, ct);
            _logger.LogInformation("Product {ProductId} created successfully", id);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        [Authorize] // JWT doğrulamalı update
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCommand body, CancellationToken ct)
        {
            if (id != body.Id)
            {
                _logger.LogWarning("Product update failed: Id mismatch. RouteId={RouteId}, BodyId={BodyId}", id, body.Id);
                return BadRequest("Id mismatch");
            }

            var ok = await _mediator.Send(body, ct);
            if (!ok)
            {
                _logger.LogWarning("Product with Id {ProductId} not found for update", id);
                return NotFound();
            }

            _logger.LogInformation("Product {ProductId} updated successfully", id);
            return NoContent();
        }
    }
}

