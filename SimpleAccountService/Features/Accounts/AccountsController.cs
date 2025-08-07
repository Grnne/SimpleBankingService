using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simple_Account_Service.Application.Models;
using Simple_Account_Service.Features.Accounts.Commands.CreateAccount;
using Simple_Account_Service.Features.Accounts.Commands.DeleteAccount;
using Simple_Account_Service.Features.Accounts.Commands.UpdateAccount;
using Simple_Account_Service.Features.Accounts.Queries.CheckAccountExists;
using Simple_Account_Service.Features.Accounts.Queries.GetAccounts;
using Simple_Account_Service.Features.Accounts.Queries.GetAccountStatement;
using Simple_Account_Service.Features.Accounts.Queries.GetAccountStatement.Dto;

namespace Simple_Account_Service.Features.Accounts;

/// <summary>
/// Контроллер для управления счетами.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]/[action]")]
public class AccountsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Создать новый счет.
    /// </summary>
    ///  <param name="createAccountDto">Данные для создания счета:
    /// <list type="bullet">
    /// <item><description>OwnerId — уникальный идентификатор владельца счета (Guid).</description></item>
    /// <item><description>Type — тип счета: Checking, Deposit, Credit.</description></item>
    /// <item><description>Currency — валюта счета в формате ISO 4217, например "USD", "EUR".</description></item>
    /// <item><description>InterestRate — процентная ставка (опционально).</description></item>
    /// </list>
    /// </param>
    /// <returns>Возвращает созданный счет с кодом 201 Created, внутри MbResult.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(MbResult<AccountDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(MbResult<string>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(MbResult<string>))]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto createAccountDto)
    {
        var response = await mediator.Send(new CreateAccountCommand(createAccountDto));

        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>
    /// Частично обновить данные счета.
    /// </summary>
    /// <param name="accountId">Идентификатор счета для обновления (Guid).</param>
    /// <param name="updatedAccountDto">Данные для обновления счета. Поля необязательны:
    /// <list type="bullet">
    /// <item><description>InterestRate — новая процентная ставка (опционально).</description></item>
    /// <item><description>ClosedAt — дата закрытия счета (опционально).</description></item>
    /// </list>
    /// </param>
    /// <returns>Возвращает обновленный счет с кодом 200 OK, внутри MbResult.</returns>
    [HttpPatch("{accountId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MbResult<AccountDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(MbResult<string>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(MbResult<string>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(MbResult<string>))]
    public async Task<IActionResult> UpdateAccount(Guid accountId, [FromBody] UpdateAccountDto updatedAccountDto)
    {
        var response = await mediator.Send(new UpdateAccountCommand(accountId, updatedAccountDto));

        return Ok(response);
    }

    /// <summary>
    /// Удалить счет по идентификатору.
    /// </summary>
    /// <param name="accountId">Идентификатор удаляемого счета (Guid).</param>
    /// <returns>Возвращает код 204 No Content при успешном удалении или ошибку внутри MbResult.</returns>
    [HttpDelete("{accountId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(MbResult<bool>))]
    public async Task<IActionResult> DeleteAccount(Guid accountId)
    {
        await mediator.Send(new DeleteAccountCommand(accountId));

        return NoContent();
    }

    /// <summary>
    /// Получить список всех счетов.
    /// </summary>
    /// <returns>Возвращает список счетов с кодом 200 OK, внутри MbResult.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MbResult<IEnumerable<AccountDto>>))]
    public async Task<IActionResult> GetAccounts()
    {
        var response = await mediator.Send(new GetAllAccountsQuery());

        return Ok(response);
    }

    /// <summary>
    /// Получить выписку по счетам владельца за заданный период.
    /// </summary>
    /// <param name="ownerId">Идентификатор владельца счетов (Guid).</param>
    /// <param name="accountId">Опциональный идентификатор счета (Guid?).</param>
    /// <param name="startDate">Дата начала периода (включительно).</param>
    /// <param name="endDate">Дата конца периода (включительно).</param>
    /// <returns>Возвращает выписку по счетам с кодом 200 OK, внутри MbResult.</returns>
    ///     /// <remarks>
    /// Пример запроса:
    ///
    ///     GET /api/Accounts/GetAccountStatement/{ownerId}?accountId={accountId}&amp;startDate=2025-01-01&amp;endDate=2025-06-30
    /// </remarks>
    [HttpGet("{ownerId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MbResult<MultiAccountStatementDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(MbResult<string>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(MbResult<string>))]
    public async Task<IActionResult> GetAccountStatement(Guid ownerId, [FromQuery] Guid? accountId,
        DateTime startDate, DateTime endDate)
    {
        var response = await mediator.Send(new GetAccountStatementQuery(ownerId, accountId, startDate, endDate));

        return Ok(response);
    }

    /// <summary>
    /// Проверить существование счета по идентификатору.
    /// </summary>
    /// <param name="accountId">Идентификатор счета (Guid).</param>
    /// <returns>Возвращает true/false с кодом 200 OK, внутри MbResult.</returns>
    [HttpGet("{accountId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MbResult<bool>))]
    public async Task<IActionResult> AccountExists(Guid accountId)
    {
        var response = await mediator.Send(new CheckAccountExistsQuery(accountId));

        return Ok(response);
    }
}
