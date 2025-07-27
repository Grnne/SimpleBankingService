using AutoMapper;
using FluentValidation;
using Simple_Account_Service.Application.Exceptions;
using Simple_Account_Service.Features.Accounts.Commands.CreateAccount;
using Simple_Account_Service.Features.Accounts.Commands.UpdateAccount;
using Simple_Account_Service.Features.Accounts.Entities;
using Simple_Account_Service.Features.Accounts.Interfaces.Repositories;
using Simple_Account_Service.Features.Accounts.Queries.GetAccountStatement.Dto;
using Simple_Account_Service.Features.Transactions.Entities;

namespace Simple_Account_Service.Features.Accounts;

public class AccountsService(IAccountRepository repository, IMapper mapper)
{
    public async Task<AccountDto> CreateAccountAsync(CreateAccountDto createAccountDto)
    {
        var account = mapper.Map<Account>(createAccountDto);

        account.Id = Guid.NewGuid();
        account.Balance = 0;
        account.CreatedAt = DateTime.UtcNow;
        account.Transactions = [];

        var response = await repository.CreateAsync(account);

        return mapper.Map<AccountDto>(response);
    }

    public async Task<AccountDto> UpdateAccountAsync(Guid accountId, UpdateAccountDto request)
    {
        var account = await repository.GetByIdAsync(accountId);

        if (account == null)
        {
            throw new NotFoundException($"Счет {accountId} не найден.");
        }

        if (account.ClosedAt != null)
        {
            throw new ConflictException("Нельзя изменить или удалить уже закрытый счет.");
        }

        if (request.ClosedAt != null)
        {
            if (request.ClosedAt < account.CreatedAt)
            {
                throw new ConflictException("Дата закрытия счета должна быть после даты открытия");
            }

            account.ClosedAt = request.ClosedAt;
        }

        if (request.InterestRate != null)
        {
            account.InterestRate = request.InterestRate;
        }

        var response = await repository.UpdateAsync(account);

        return mapper.Map<AccountDto>(response);
    }

    public async Task DeleteAccountAsync(Guid accountId)
    {
        var account = await repository.GetByIdAsync(accountId);

        if (account == null)
        {
            throw new NotFoundException($"Счет с айди {accountId} не найден.");
        }

        await repository.DeleteAsync(accountId);
    }

    public async Task<IEnumerable<AccountDto>> GetAllAccountsAsync()
    {
        var accounts = await repository.GetAllAccountsAsync();
        return mapper.Map<IEnumerable<AccountDto>>(accounts);
    }

    // Фильтрация здесь, пока нет нормальной базы данных
    // Алгоритм поиска остатка на начало выписки здесь, т.к. по тз сущности не позволяют сделать адекватно
    // В любом случае тут должен быть рефактор на балансы, и разодрать всё на методы

    public async Task<MultiAccountStatementDto> GetAccountStatement(Guid ownerId, Guid? accountId,
        DateTime startDate, DateTime endDate)
    {
        var accounts = await repository.GetAllAccountsAsync();
        var filteredOwnerAccounts = accounts.Where(a => a.OwnerId == ownerId).ToList();

        if (!filteredOwnerAccounts.Any())
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

            if (!filteredAccounts.Any())
            {
                throw new NotFoundException($"В данном периоде не найден счет с {accountId}.");
            }

            filteredOwnerAccounts = filteredAccounts;
        }
        else
        {
            filteredOwnerAccounts = FilterAccountsByPeriod(filteredOwnerAccounts, startDate, endDate);

            if (!filteredOwnerAccounts.Any())
            {
                throw new NotFoundException($"Счета владельца с id {ownerId} не найдены в заданном периоде.");
            }
        }

        // Часть логики можно в принципе в маппер скинуть
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

        return multiAccountStatement;
    }

    public async Task<bool> CheckAccountExists(Guid accountId)
    {
        var result = await repository.GetByIdAsync(accountId);

        return result != null;
    }

    // Хотел разбить по дяде Бобу все на методы поменьше, но т.к. все равно надо переделывать целиком, оставил так
    private static List<Account> FilterAccountsByPeriod(List<Account> accounts, DateTime startDate, DateTime endDate)
    {
        return accounts
            .Where(a => a.CreatedAt <= endDate && (a.ClosedAt == null || a.ClosedAt >= startDate))
            .ToList();
    }
}