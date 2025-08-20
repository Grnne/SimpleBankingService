using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simple_Account_Service.Application.Models;
using Simple_Account_Service.Features.Transactions.Commands.CreateTransaction;
using Simple_Account_Service.Features.Transactions.Commands.TransferBetweenAccounts;

namespace Simple_Account_Service.Features.Transactions;

/// <summary>
/// Контроллер для управления транзакциями на счетах.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]/[action]/{accountId:guid}")]
public class TransactionsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Создать транзакцию на указанном счете.
    /// </summary>
    /// <param name="accountId">Идентификатор счета (Guid), на котором создаётся транзакция.</param>
    /// <param name="createTransactionDto">Данные создаваемой транзакции:
    /// <list type="bullet">
    /// <item><description>Amount — сумма транзакции (decimal).</description></item>
    /// <item><description>Currency — валюта (ISO 4217), например "USD", "EUR".</description></item>
    /// <item><description>Type — тип транзакции, Credit или Debit.</description></item>
    /// <item><description>Description — описание/комментарий (string, nullable).</description></item>
    /// </list>
    /// </param>
    /// <returns>Возвращает результат операции, упакованный в MbResult, содержащий созданную транзакцию. При успехе HTTP статус 201 Created.</returns>
    /// <remarks>
    /// Пример запроса:
    /// 
    ///     POST /api/Transactions/CreateTransaction/{accountId}
    ///     {
    ///         "amount": 100.50,
    ///         "currency": "USD",
    ///         "type": 1,
    ///         "description": "Payment for invoice #123"
    ///     }
    /// </remarks>
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(MbResult<TransactionDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(MbResult<string>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(MbResult<string>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(MbResult<string>))]
    [HttpPost]
    public async Task<IActionResult> CreateTransaction(Guid accountId, [FromBody] CreateTransactionDto createTransactionDto)
    {

        var correlationId = GetOrCreateCorrelationId();
        var response = await mediator.Send(
            new CreateTransactionCommand(accountId, createTransactionDto, correlationId));

        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>
    /// Перевести средства между счетами.
    /// </summary>
    /// <param name="accountId">Идентификатор счета-источника (Guid).</param>
    /// <param name="transferDto">Параметры перевода:
    /// <list type="bullet">
    /// <item><description>Amount — сумма перевода (decimal).</description></item>
    /// <item><description>Currency — валюта перевода (ISO 4217), например "EUR", "USD".</description></item>
    /// <item><description>Type — тип транзакции, Credit или Debit.</description></item>
    /// <item><description>Description — описание перевода (string, nullable).</description></item>
    /// </list>
    /// </param>
    /// <returns>Возвращает результат операции, упакованный в MbResult, содержащий список транзакций. При успехе HTTP статус 200 OK.</returns>
    /// <remarks>
    /// Пример запроса:
    /// 
    ///     POST /api/Transactions/TransferBetweenAccounts/{accountId}
    ///     {
    ///         "amount": 250.00,
    ///         "currency": "EUR",
    ///         "type": 0,
    ///         "description": "Transfer to savings"
    ///     }
    /// </remarks>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MbResult<List<TransactionDto>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(MbResult<string>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(MbResult<string>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(MbResult<string>))]
    [HttpPost]
    public async Task<IActionResult> TransferBetweenAccounts(Guid accountId, [FromBody] TransferDto transferDto)
    {
        var correlationId = GetOrCreateCorrelationId();
        var response = await mediator.Send(
            new TransferBetweenAccountsCommand(accountId, transferDto, correlationId));

        return Ok(response);
    }

    private Guid GetOrCreateCorrelationId()
    {
        if (Request.Headers.TryGetValue("X-Correlation-Id", out var cidString)
            && Guid.TryParse(cidString, out var cid))
        {
            return cid;
        }
        return Guid.NewGuid();
    }
}

