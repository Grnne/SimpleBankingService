using AutoMapper;
using JetBrains.Annotations;
using MediatR;
using Simple_Account_Service.Application.Models;
using Simple_Account_Service.Features.Accounts.Entities;
using Simple_Account_Service.Features.Accounts.Interfaces.Repositories;

namespace Simple_Account_Service.Features.Accounts.Commands.CreateAccount;

[UsedImplicitly]
public class CreateAccountCommandHandler(IAccountRepository repository, IMapper mapper) : IRequestHandler<CreateAccountCommand, MbResult<AccountDto>>
{
    public async Task<MbResult<AccountDto>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = mapper.Map<Account>(request.Request);

        account.Id = Guid.NewGuid();
        account.Balance = 0;
        account.CreatedAt = DateTime.UtcNow;
        account.Transactions = [];

        var response = await repository.CreateAsync(account);

        return new MbResult<AccountDto>(mapper.Map<AccountDto>(response));
    }
}