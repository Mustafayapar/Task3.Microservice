using Log.Api.Application.Commands;
using Log.Api.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Log.Api.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class LogsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<LogsController> _logger;

        public LogsController(IMediator mediator, ILogger<LogsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost] // Manuel log kaydı (opsiyonel)
        public async Task<IActionResult> Create([FromBody] CreateLogCommand cmd, CancellationToken ct)
        {
            _logger.LogInformation("Received manual log create request: {@Command}", cmd);

            try
            {
                var id = await _mediator.Send(cmd, ct);
                _logger.LogInformation("Log entry {LogId} created successfully", id);

                return CreatedAtAction(nameof(Get), new { id }, new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create log entry: {@Command}", cmd);
                return StatusCode(500, "Unexpected error occurred while creating log");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string? serviceName,
            [FromQuery] string? level,
            [FromQuery] DateTime? fromUtc,
            [FromQuery] DateTime? toUtc,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken ct = default)
        {
            _logger.LogInformation(
                "Fetching logs. ServiceName={ServiceName}, Level={Level}, FromUtc={FromUtc}, ToUtc={ToUtc}, Page={Page}, PageSize={PageSize}",
                serviceName, level, fromUtc, toUtc, page, pageSize);

            try
            {
                var result = await _mediator.Send(new GetLogsQuery(serviceName, level, fromUtc, toUtc, page, pageSize), ct);
                _logger.LogInformation("Fetched {Count} logs");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch logs with filters ServiceName={ServiceName}, Level={Level}", serviceName, level);
                return StatusCode(500, "Unexpected error occurred while fetching logs");
            }

        }
    }
}