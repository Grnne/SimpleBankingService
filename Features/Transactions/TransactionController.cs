using MediatR;
using Microsoft.AspNetCore.Mvc;
using Simple_Account_Service.Features.Transactions.Entitites;

namespace Simple_Account_Service.Features.Transactions;

[ApiController]
[Route("api/[controller]/[action]")]
public class TransactionController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPost("{accountId:guid}")]
    public IActionResult RegisterTransaction(Guid accountId, [FromBody] Transaction transaction)
    {
        return Ok();
    }

    [HttpPost("transfer")]
    public IActionResult Transfer([FromBody] string request)
    {
        return Ok();
    }
}