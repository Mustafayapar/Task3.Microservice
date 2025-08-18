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
    public ProductsController(MediatR.IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _mediator.Send(new GetProductsQuery(), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var item = await _mediator.Send(new GetProductByIdQuery(id), ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost] // Ürün Ekleme (opsiyonel Authorize)
    public async Task<IActionResult> Create([FromBody] CreateProductCommand cmd, CancellationToken ct)
    {
        var id = await _mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [Authorize] // Gereksinim: Update JWT doğrulamalı
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCommand body, CancellationToken ct)
    {
        if (id != body.Id) return BadRequest("Id mismatch");
        var ok = await _mediator.Send(body, ct);
        return ok ? NoContent() : NotFound();
    }
}
}
