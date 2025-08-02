using AutoMapper;
using JetBrains.Annotations;
using MediatR;
using Simple_Account_Service.Application.Exceptions;
using Simple_Account_Service.Application.Models;
using Simple_Account_Service.Features.Accounts.Interfaces.Repositories;

namespace Simple_Account_Service.Features.Accounts.Commands.UpdateAccount;

[UsedImplicitly]
public class UpdateAccountCommandHandler(IAccountRepository repository, IMapper mapper) : IRequestHandler<UpdateAccountCommand, MbResult<AccountDto>>
{
    public async Task<MbResult<AccountDto>> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.AccountId);

        if (account == null)
        {
            throw new NotFoundException($"Счет {request.AccountId} не найден.");
        }

        if (account.ClosedAt != null)
        {
            throw new ConflictException("Нельзя изменить или удалить уже закрытый счет.");
        }

        if (request.Request.ClosedAt != null)
        {
            if (request.Request.ClosedAt < account.CreatedAt)
            {
                throw new ConflictException("Дата закрытия счета должна быть после даты открытия");
            }

            account.ClosedAt = request.Request.ClosedAt;
        }

        if (request.Request.InterestRate != null)
        {
            account.InterestRate = request.Request.InterestRate;
        }

        var response = await repository.UpdateAsync(account);

        return new MbResult<AccountDto>(mapper.Map<AccountDto>(response));
    }
}