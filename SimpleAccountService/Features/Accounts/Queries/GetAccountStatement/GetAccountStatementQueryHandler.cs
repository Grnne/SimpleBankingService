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
public class GetAccountStatementQueryHandler(IAccountRepository repository, IMapper mapper)
    : IRequestHandler<GetAccountStatementQuery, MbResult<MultiAccountStatementDto>>
{
    public async Task<MbResult<MultiAccountStatementDto>> Handle(GetAccountStatementQuery request, CancellationToken cancellationToken)
    {

        // Хочется сделать проекции в репозитории, возможно стоит refactor, но имеет ли вообще это смысл,
        // учитывая что надо делать промежуточные балансы по месяцам, например
        var ownerId = request.OwnerId;
        var startDate = request.StartDate;
        var endDate = request.EndDate;
        var accountId = request.AccountId;

        var filteredAccounts = (await repository.GetAccountsEagerlyUpToEndDateByOwnerAsync(ownerId, endDate))
            .ToList();

        filteredAccounts = FilterAndValidateAccounts(filteredAccounts, ownerId, startDate, endDate, accountId);

        var balancesDict = GetStartEndBalances(filteredAccounts, startDate, endDate);

        var accountsStatements = filteredAccounts.Select(account =>
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

    private static List<Account> FilterAccountsByPeriod(List<Account> accounts, DateTime startDate, DateTime endDate)
    {
        var result = accounts
            .Where(a => a.CreatedAt <= endDate && (a.ClosedAt == null || a.ClosedAt >= startDate))
            .ToList();

        return result;
    }

    private static Dictionary<Guid, (decimal startBalance, decimal endBalance)> GetStartEndBalances(
        List<Account> accounts,
        DateTime startDate,
        DateTime endDate)
    {
        var balancesDict = new Dictionary<Guid, (decimal startBalance, decimal endBalance)>();

        foreach (var account in accounts)
        {
            var startBalance = account.Transactions
                .Where(t => t.Timestamp < startDate)
                .Sum(t => t.Type == TransactionType.Credit ? t.Amount : -t.Amount);

            var endBalance = account.Transactions
                .Where(t => t.Timestamp <= endDate)
                .Sum(t => t.Type == TransactionType.Credit ? t.Amount : -t.Amount);

            balancesDict.Add(account.Id, (startBalance, endBalance));
        }

        return balancesDict;
    }

    private static List<Account> FilterAndValidateAccounts(
        List<Account> accounts,
        Guid ownerId,
        DateTime startDate,
        DateTime endDate,
        Guid? accountId = null)
    {
        if (accounts == null || accounts.Count == 0)
        {
            throw new NotFoundException($"Счета владельца с id {ownerId} в данном периоде не найдены.");
        }

        if (accountId != null)
        {
            var existing = accounts.Find(a => a.Id == accountId);

            if (existing == null)
            {
                throw new NotFoundException($"В данном периоде не найден счет с {accountId}.");
            }

            if (existing.OwnerId != ownerId)
            {
                throw new ValidationException($"Счет с id {accountId} не принадлежит владельцу с id {ownerId}.");
            }

            var filtered = FilterAccountsByPeriod([existing], startDate, endDate);

            if (filtered.Count == 0)
            {
                throw new NotFoundException($"В данном периоде не найден счет с {accountId}.");
            }

            return filtered;
        }
        else
        {
            var filtered = FilterAccountsByPeriod(accounts, startDate, endDate);

            if (filtered.Count == 0)
            {
                throw new NotFoundException($"Счета владельца с id {ownerId} не найдены в заданном периоде.");
            }

            return filtered;
        }
    }
}