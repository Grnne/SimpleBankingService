using Microsoft.EntityFrameworkCore;
using Simple_Account_Service.Application.ForFakesAndDummies;
using Simple_Account_Service.Features.Accounts.Entities;
using Simple_Account_Service.Features.Transactions.Entities;

namespace Simple_Account_Service.Infrastructure.Data;

public static class DataSeeder
{
    public static void SeedFakeData(SasDbContext context, FakeDb fakeDb)
    {
        // Check for existing data synchronously from async call
        if (context.Accounts.AnyAsync().GetAwaiter().GetResult())
            return; // Data already present

        // Fetch owners from FakeDb similarly
        var owners = fakeDb.GetAllOwnersAsync().GetAwaiter().GetResult().ToList();

        if (owners.Count == 0)
            throw new InvalidOperationException("FakeDb contains an empty owners list.");

        // Fixed GUIDs for accounts
        var accountId1 = Guid.Parse("01111111-1111-1111-1111-111111111111");
        var accountId2 = Guid.Parse("02222222-2222-2222-2222-222222222222");
        var accountId3 = Guid.Parse("03333333-3333-3333-3333-333333333333");
        var accountId4 = Guid.Parse("04444444-4444-4444-4444-444444444444");

        // Fixed GUIDs for transactions
        var transactionId1 = Guid.Parse("91111111-1111-1111-1111-111111111111");
        var transactionId2 = Guid.Parse("92222222-2222-2222-2222-222222222222");
        var transactionId3 = Guid.Parse("93333333-3333-3333-3333-333333333333");
        var transactionId4 = Guid.Parse("94444444-4444-4444-4444-444444444444");

        var accounts = new List<Account>
        {
            new()
            {
                Id = accountId1,
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
                Id = accountId2,
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
                Id = accountId3,
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
                Id = accountId4,
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
                Id = transactionId1,
                AccountId = accountId1,
                CounterpartyAccountId = null,
                Amount = 1000.00m,
                Currency = "USD",
                Type = TransactionType.Credit,
                Description = "Initial deposit",
                Timestamp = DateTime.UtcNow.AddMonths(-3)
            },
            new()
            {
                Id = transactionId2,
                AccountId = accountId1,
                CounterpartyAccountId = accountId2,
                Amount = 200.00m,
                Currency = "USD",
                Type = TransactionType.Debit,
                Description = "Transfer to EUR deposit",
                Timestamp = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = transactionId3,
                AccountId = accountId2,
                CounterpartyAccountId = accountId1,
                Amount = 200.00m,
                Currency = "EUR",
                Type = TransactionType.Credit,
                Description = "Received from USD checking account",
                Timestamp = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = transactionId4,
                AccountId = accountId3,
                CounterpartyAccountId = null,
                Amount = 500.00m,
                Currency = "RUB",
                Type = TransactionType.Debit,
                Description = "Credit card payment",
                Timestamp = DateTime.UtcNow.AddMonths(-1)
            }
        };

        // Link for navigation properties
        accounts[0].Transactions.Add(transactions[0]);
        accounts[0].Transactions.Add(transactions[1]);
        accounts[1].Transactions.Add(transactions[2]);
        accounts[2].Transactions.Add(transactions[3]);

        // Add to context and save synchronously calling async methods
        context.Accounts.AddRange(accounts);
        context.Transactions.AddRange(transactions);

        context.SaveChangesAsync().GetAwaiter().GetResult();
    }
}