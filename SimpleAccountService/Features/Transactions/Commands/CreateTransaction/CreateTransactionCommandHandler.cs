using AutoMapper;
using JetBrains.Annotations;
using MediatR;
using Simple_Account_Service.Application.Models;
using Simple_Account_Service.Features.Accounts.Interfaces.Repositories;
using Simple_Account_Service.Features.Transactions.Entities;
using Simple_Account_Service.Features.Transactions.Interfaces;
using Simple_Account_Service.Features.Transactions.Interfaces.Repositories;

namespace Simple_Account_Service.Features.Transactions.Commands.CreateTransaction;

[UsedImplicitly]
public class CreateTransactionCommandHandler(ITransactionRepository transactionRepository, IAccountRepository accountRepository, ITransactionService service, IMapper mapper) : IRequestHandler<CreateTransactionCommand, MbResult<TransactionDto>>
{
    public async Task<MbResult<TransactionDto>> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var accountId = request.AccountId;
        var createTransactionDto = request.CreateTransactionDto;
        var account = await accountRepository.GetByIdAsync(accountId);

        service.CheckAccount(accountId, account, createTransactionDto.Type, createTransactionDto.Amount, createTransactionDto.Currency);

        var transaction = mapper.Map<Transaction>(createTransactionDto);
        transaction.Id = Guid.NewGuid();
        transaction.AccountId = accountId;
        transaction.Timestamp = DateTime.UtcNow;

        var result = await transactionRepository.CreateAsync(transaction);

        switch (createTransactionDto.Type)
        {
            case TransactionType.Debit:
                account.Balance -= createTransactionDto.Amount;
                break;
            case TransactionType.Credit:
                account.Balance += createTransactionDto.Amount;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(request));
        }
        await accountRepository.UpdateAsync(account);

        return new MbResult<TransactionDto>(mapper.Map<TransactionDto>(result));
    }
}