using MediatR;
using Microsoft.AspNetCore.Mvc;
using Simple_Account_Service.Features.Transactions.Commands.CreateTransaction;
using Simple_Account_Service.Features.Transactions.Commands.TransferBetweenAccounts;

namespace Simple_Account_Service.Features.Transactions;

[ApiController]
[Route("api/[controller]/[action]/{accountId:guid}")]
public class TransactionController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPost("")]
    public IActionResult CreateTransaction(Guid accountId, [FromBody] CreateTransactionDto createTransactionDto)
    {
        return Ok();
    }

    [HttpPost("")]
    public IActionResult TransferBetweenAccounts(Guid accountId, [FromBody] TransferBetweenAccountsDto createTransactionDto)
    {
        return Ok();
    }
}