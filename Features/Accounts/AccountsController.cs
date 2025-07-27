using MediatR;
using Microsoft.AspNetCore.Mvc;
using Simple_Account_Service.Features.Accounts.Commands.CreateAccount;
using Simple_Account_Service.Features.Accounts.Commands.DeleteAccount;
using Simple_Account_Service.Features.Accounts.Commands.UpdateAccount;
using Simple_Account_Service.Features.Accounts.Queries.CheckAccountExists;
using Simple_Account_Service.Features.Accounts.Queries.GetAccounts;
using Simple_Account_Service.Features.Accounts.Queries.GetAccountStatement;

namespace Simple_Account_Service.Features.Accounts;

[ApiController]
[Route("api/[controller]/[action]")]
public class AccountsController(IMediator mediator) : ControllerBase
{
    //todo как в тз
    [HttpPost]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto createAccountDto)
    {
        var response = await mediator.Send(new CreateAccountCommand(createAccountDto));

        return Ok(response);
    }

    [HttpPut("{accountId:guid}")]
    public async Task<IActionResult> UpdateAccount(Guid accountId, [FromBody] UpdateAccountDto updatedAccountDto)
    {
        var response = await mediator.Send(
            new UpdateAccountCommand(accountId, updatedAccountDto));

        return Ok(response);
    }

    [HttpDelete("{accountId:guid}")]
    public async Task<IActionResult> DeleteAccount(Guid accountId)
    {
        await mediator.Send(new DeleteAccountCommand(accountId));

        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> GetAccounts()
    {
        var response = await mediator.Send(new GetAllAccountsQuery());

        return Ok(response.ToList());
    }


    [HttpGet("{ownerId:guid}")]
    public async Task<IActionResult> GetAccountStatement(Guid ownerId, [FromQuery] Guid? accountId,
        DateTime startDate, DateTime endDate)
    {

        var response = await mediator.Send(
            new GetAccountStatementQuery(ownerId, accountId, startDate, endDate));

        return Ok(response);
    }


    //Посчитал излишним делать queries валидаторы только для пустого гуид, неконсистентно и уродливо, стоит поправить наверное
    [HttpGet("{accountId:guid}")]
    public async Task<IActionResult> AccountExists(Guid accountId)
    {
        if (accountId == Guid.Empty)
        {
            return BadRequest("Идентификатор владельца не может быть пустым.");
        }

        var response = await mediator.Send(new CheckAccountExistsQuery(accountId));

        return Ok(response);
    }
}