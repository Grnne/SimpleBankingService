using AutoMapper;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using Simple_Account_Service.Application.Exceptions;
using Simple_Account_Service.Application.Models;
using Simple_Account_Service.Features.Accounts.Entities;
using Simple_Account_Service.Features.Accounts.Interfaces.Repositories;
using Simple_Account_Service.Features.Accounts.Queries.GetAccountStatement.Dto;
using Simple_Account_Service.Features.Transactions.Entities;

namespace Simple_Account_Service.Features.Accounts.Queries.GetAccountStatement;

[UsedImplicitly]
public class GetAccountStatementQueryHandler(IAccountRepository repository, IMapper mapper) : IRequestHandler<GetAccountStatementQuery, MbResult<MultiAccountStatementDto>>
{
    public async Task<MbResult<MultiAccountStatementDto>> Handle(GetAccountStatementQuery request, CancellationToken cancellationToken)
    {
        var accounts = await repository.GetAllAccountsAsync();

        var ownerId = request.OwnerId;
        var startDate = request.StartDate;
        var endDate = request.EndDate;
        var accountId = request.AccountId;

        var filteredOwnerAccounts = accounts.Where(a => a.OwnerId == ownerId).ToList();

        if (filteredOwnerAccounts.Count == 0)
        {
            throw new NotFoundException($"Счета владельца с id {ownerId} не найдены.");
        }

        var balancesDict = new Dictionary<Guid, (decimal startBalance, decimal endBalance)>();

        foreach (var account in filteredOwnerAccounts)
        {
            var startBalance = account.Transactions
                .Where(t => t.Timestamp < startDate)
                .Sum(t => t.Type == TransactionType.Credit ? t.Amount : -t.Amount);

            var endBalance = account.Transactions
                .Where(t => t.Timestamp <= endDate)
                .Sum(t => t.Type == TransactionType.Credit ? t.Amount : -t.Amount);

            balancesDict.Add(account.Id, (startBalance, endBalance));
        }

        if (accountId != null)
        {
            var existing = await repository.GetByIdAsync(accountId.Value);

            if (existing == null)
            {
                throw new NotFoundException($"В данном периоде не найден счет с {accountId}.");
            }

            if (existing.OwnerId != ownerId)
            {
                throw new ValidationException($"Счет с id {accountId} не принадлежит владельцу с id {ownerId}.");
            }

            var filteredAccounts = FilterAccountsByPeriod([existing], startDate, endDate);

            if (filteredAccounts.Count == 0)
            {
                throw new NotFoundException($"В данном периоде не найден счет с {accountId}.");
            }

            filteredOwnerAccounts = filteredAccounts;
        }
        else
        {
            filteredOwnerAccounts = FilterAccountsByPeriod(filteredOwnerAccounts, startDate, endDate);

            if (filteredOwnerAccounts.Count == 0)
            {
                throw new NotFoundException($"Счета владельца с id {ownerId} не найдены в заданном периоде.");
            }
        }

        var accountsStatements = filteredOwnerAccounts.Select(account =>
        {
            var filteredTransactions = account.Transactions
                .Where(t => t.Timestamp >= startDate && t.Timestamp <= endDate)
                .ToList();

            return new AccountStatementDto
            {
                Id = account.Id,
                Type = account.Type,
                Currency = account.Currency,
                StartingBalance = balancesDict[account.Id].startBalance,
                EndingBalance = balancesDict[account.Id].endBalance,
                InterestRate = account.InterestRate,
                Transactions = mapper.Map<List<TransactionForStatementDto>>(filteredTransactions)
            };
        })
            .ToList();

        var multiAccountStatement = new MultiAccountStatementDto
        {
            OwnerId = ownerId,
            PeriodStart = startDate,
            PeriodEnd = endDate,
            TotalStartingBalance = balancesDict.Values.Sum(v => v.startBalance),
            TotalEndingBalance = balancesDict.Values.Sum(v => v.endBalance),
            AccountStatements = accountsStatements
        };

        return new MbResult<MultiAccountStatementDto>(multiAccountStatement);
    }

    // Хотел разбить по дяде Бобу все на методы поменьше, но т.к. все равно надо переделывать, оставил так
    private static List<Account> FilterAccountsByPeriod(List<Account> accounts, DateTime startDate, DateTime endDate)
    {
        var result = accounts
            .Where(a => a.CreatedAt <= endDate && (a.ClosedAt == null || a.ClosedAt >= startDate)).ToList();

        return result;
    }
}