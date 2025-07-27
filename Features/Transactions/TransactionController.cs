using MediatR;
using Microsoft.AspNetCore.Mvc;
using Simple_Account_Service.Features.Transactions.Commands.CreateTransaction;
using Simple_Account_Service.Features.Transactions.Commands.TransferBetweenAccounts;

namespace Simple_Account_Service.Features.Transactions;

[ApiController]
[Route("api/[controller]/[action]/{accountId:guid}")]
public class TransactionController(IMediator mediator) : ControllerBase
{
    [HttpPost("")]
    public async Task<IActionResult> CreateTransaction(Guid accountId, [FromBody] CreateTransactionDto createTransactionDto)
    {
        var response = await mediator.Send(
            new CreateTransactionCommand(accountId, createTransactionDto));

        return Ok(response);
    }

    [HttpPost("")]
    public async Task<IActionResult> TransferBetweenAccounts(Guid accountId, [FromBody] TransferDto transferDto)
    {
        var response = await mediator.Send(
            new TransferBetweenAccountsCommand(accountId, transferDto));

        return Ok(response);
    }
}