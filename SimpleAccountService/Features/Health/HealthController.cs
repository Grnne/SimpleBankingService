using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Simple_Account_Service.Application.Models;

namespace Simple_Account_Service.Features.Health;

/// <summary>
/// Контроллер для проверки состояния сервиса (health checks).
/// </summary>
[ApiController]
[Route("api/[controller]/[action]")]
public class HealthController(HealthCheckService health, IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Проверка живости сервиса (liveness probe).
    /// Возвращает HTTP 200 OK, если сервис запущен и работает.
    /// </summary>
    /// <returns>HTTP 200 OK без тела.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Live()
    {

        return Ok(await health.CheckHealthAsync());
    }

    /// <summary>
    /// Проверка готовности сервиса (readiness probe).
    /// Проверяет внутренние зависимости, например, состояние очереди Outbox.
    /// Возвращает HTTP 200 OK при готовности, HTTP 503 при проблемах с состоянием.
    /// </summary>
    /// <returns>HTTP 200 OK с информацией о готовности или 503 с ошибкой.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(MbError), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Ready()
    {
        var result = await mediator.Send(new ReadyQuery());

        return result is { Success: false, Error: not null }
            ? StatusCode((int)result.Error.Code, result.Error)
            : Ok(result.Response);
    }
}