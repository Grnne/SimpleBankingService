using MediatR;
using Microsoft.AspNetCore.Mvc;
using Simple_Account_Service.Features.Accounts.Commands.AddAccount;
using Simple_Account_Service.Features.Accounts.Queries.GetAccountsList;
using Simple_Account_Service.Features.Accounts.Queries.GetAccountStatement;

namespace Simple_Account_Service.Features.Accounts
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AccountsController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpPost]
        public async Task<ActionResult> CreateAccount([FromBody] Account account)
        {
            await _mediator.Send(new AddAccount(account));

            return StatusCode(201);
        }

        [HttpPut("{id:guid}")]
        public IActionResult UpdateAccount(Guid id, [FromBody] Account updatedAccount)
        {
            return Ok();
        }

        [HttpDelete("{id:guid}")]
        public IActionResult DeleteAccount(Guid id)
        {
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetAccounts()
        {
            var response = await _mediator.Send(new GetAccountsList());

            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetAccountById(Guid id)
        {
            var response = await _mediator.Send(new GetAccountStatement(id));

            return Ok();
        }

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

        [HttpGet("{ownerId:guid}")]
        public IActionResult GetStatement(Guid ownerId)
        {
            return Ok();
        }

        [HttpGet]
        public IActionResult AccountExists([FromQuery] Guid ownerId)
        {
            return Ok();
        }
    }
}