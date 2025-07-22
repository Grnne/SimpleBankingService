using Simple_Account_Service.Features.Accounts;

namespace Simple_Account_Service.Infrastructure.InMemory;

public class FakeDb
{
    private static List<Account> _accounts = [];

    public FakeDb()
    {
        _accounts =
        [
            new Account
            {
                Id = Guid.NewGuid(),
                OwnerId = Guid.NewGuid(),
                Тип = AccountType.Checking,
                Currency = "USD",
                Balance = 1000.50m,
                InterestRate = null,
                CreatedAt = DateTime.UtcNow.AddMonths(-6),
                ClosedAt = null,
                Transactions =
                [
                    new Transaction
                    {
                        Id = Guid.NewGuid(),
                        AccountId = Guid.NewGuid(),
                        CounterpartyAccountId = null,
                        Amount = 200.00m,
                        Currency = "USD",
                        Type = TransactionType.Credit,
                        Description = "Initial deposit",
                        Timestamp = DateTime.UtcNow.AddMonths(-5)
                    },

                    new Transaction
                    {
                        Id = Guid.NewGuid(),
                        AccountId = Guid.NewGuid(),
                        CounterpartyAccountId = null,
                        Amount = 50.00m,
                        Currency = "USD",
                        Type = TransactionType.Debit,
                        Description = "Grocery shopping",
                        Timestamp = DateTime.UtcNow.AddMonths(-3)
                    }
                ]
            },


            new Account
            {
                Id = Guid.NewGuid(),
                OwnerId = Guid.NewGuid(),
                Тип = AccountType.Deposit,
                Currency = "EUR",
                Balance = 5000m,
                InterestRate = 0.03m,
                CreatedAt = DateTime.UtcNow.AddYears(-1),
                ClosedAt = null,
                Transactions =
                [
                    new Transaction
                    {
                        Id = Guid.NewGuid(),
                        AccountId = Guid.NewGuid(),
                        CounterpartyAccountId = null,
                        Amount = 1000m,
                        Currency = "EUR",
                        Type = TransactionType.Credit,
                        Description = "Deposit top-up",
                        Timestamp = DateTime.UtcNow.AddMonths(-11)
                    }
                ]
            },


            new Account
            {
                Id = Guid.NewGuid(),
                OwnerId = Guid.NewGuid(),
                Тип = AccountType.Credit,
                Currency = "GBP",
                Balance = -1500m,
                InterestRate = 0.07m,
                CreatedAt = DateTime.UtcNow.AddMonths(-3),
                ClosedAt = null,
                Transactions =
                [
                    new Transaction
                    {
                        Id = Guid.NewGuid(),
                        AccountId = Guid.NewGuid(),
                        CounterpartyAccountId = null,
                        Amount = 500m,
                        Currency = "GBP",
                        Type = TransactionType.Debit,
                        Description = "Credit card purchase",
                        Timestamp = DateTime.UtcNow.AddMonths(-2)
                    }
                ]
            }
        ];

        foreach (var account in _accounts)
        {
            foreach (var txn in account.Transactions)
            {
                txn.AccountId = account.Id;
            }
        }
    }

    public async Task AddAccount(Account account)
    {
        _accounts.Add(account);
        await Task.CompletedTask;
    }

    public async Task<IEnumerable<Account>> GetAllProducts() => await Task.FromResult(_accounts);
}