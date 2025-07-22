using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Simple_Account_Service.Features.Accounts
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AccountsController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpPost]
        public IActionResult CreateAccount([FromBody] Account account)
        {
            return Ok();
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
        public IActionResult GetAccounts()
        {
            return Ok();
        }

        [HttpGet("{id:guid}")]
        public IActionResult GetAccountById(Guid id)
        {
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