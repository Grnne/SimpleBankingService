using Microsoft.EntityFrameworkCore;
using Simple_Account_Service.Application.ForFakesAndDummies;
using Simple_Account_Service.Features.Accounts.Entities;
using Simple_Account_Service.Features.Transactions.Entities;

namespace Simple_Account_Service.Infrastructure.Data;

public static class DataSeeder
{
    public static void SeedFakeData(SasDbContext context, FakeDb fakeDb)
    {
        // Проверяем наличие данных, вызывая асинхронно, но дожидаясь синхронно результата
        if (context.Accounts.AnyAsync().GetAwaiter().GetResult())
            return; // Данные уже есть

        // Берём владельцев из FakeDb аналогично
        var owners = fakeDb.GetAllOwnersAsync().GetAwaiter().GetResult().ToList();

        if (owners.Count == 0)
            throw new InvalidOperationException("FakeDb содержит пустой список владельцев.");

        var accounts = new List<Account>
        {
            new()
            {
                Id = Guid.NewGuid(),
                OwnerId = owners[0].Id,
                Type = AccountType.Checking,
                Currency = "USD",
                Balance = 1500.00m,
                InterestRate = null,
                CreatedAt = DateTime.UtcNow.AddMonths(-4),
                ClosedAt = null,
                Transactions = []
            },
            new()
            {
                Id = Guid.NewGuid(),
                OwnerId = owners[0].Id,
                Type = AccountType.Deposit,
                Currency = "EUR",
                Balance = 5000.00m,
                InterestRate = 0.03m,
                CreatedAt = DateTime.UtcNow.AddMonths(-12),
                ClosedAt = null,
                Transactions = []
            },
            new()
            {
                Id = Guid.NewGuid(),
                OwnerId = owners[1].Id,
                Type = AccountType.Credit,
                Currency = "RUB",
                Balance = 2000.00m,
                InterestRate = 0.07m,
                CreatedAt = DateTime.UtcNow.AddMonths(-2),
                ClosedAt = null,
                Transactions = []
            },
            new()
            {
                Id = Guid.NewGuid(),
                OwnerId = owners[1].Id,
                Type = AccountType.Credit,
                Currency = "RUB",
                Balance = 2000.00m,
                InterestRate = 0.07m,
                CreatedAt = DateTime.UtcNow.AddMonths(-1),
                ClosedAt = null,
                Transactions = []
            }
        };

        var transactions = new List<Transaction>
        {
            new()
            {
                Id = Guid.NewGuid(),
                AccountId = accounts[0].Id,
                CounterpartyAccountId = null,
                Amount = 1000.00m,
                Currency = "USD",
                Type = TransactionType.Credit,
                Description = "Initial deposit",
                Timestamp = DateTime.UtcNow.AddMonths(-3)
            },
            new()
            {
                Id = Guid.NewGuid(),
                AccountId = accounts[0].Id,
                CounterpartyAccountId = accounts[1].Id,
                Amount = 200.00m,
                Currency = "USD",
                Type = TransactionType.Debit,
                Description = "Transfer to EUR deposit",
                Timestamp = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = Guid.NewGuid(),
                AccountId = accounts[1].Id,
                CounterpartyAccountId = accounts[0].Id,
                Amount = 200.00m,
                Currency = "EUR",
                Type = TransactionType.Credit,
                Description = "Received from USD checking account",
                Timestamp = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = Guid.NewGuid(),
                AccountId = accounts[2].Id,
                CounterpartyAccountId = null,
                Amount = 500.00m,
                Currency = "RUB",
                Type = TransactionType.Debit,
                Description = "Credit card payment",
                Timestamp = DateTime.UtcNow.AddMonths(-1)
            }
        };

        // Связываем для навигации
        accounts[0].Transactions.Add(transactions[0]);
        accounts[0].Transactions.Add(transactions[1]);
        accounts[1].Transactions.Add(transactions[2]);
        accounts[2].Transactions.Add(transactions[3]);

        // Добавляем в контекст синхронно вызывая асинхронное API через GetAwaiter().GetResult()
        context.Accounts.AddRange(accounts);
        context.Transactions.AddRange(transactions);

        context.SaveChangesAsync().GetAwaiter().GetResult();
    }
}