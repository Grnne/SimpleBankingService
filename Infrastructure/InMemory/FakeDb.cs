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
                Transactions = []
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
                Transactions = []
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
                Transactions = []
            }
        ];
    }

    public async Task AddProduct(Account account)
    {
        _accounts.Add(account);
        await Task.CompletedTask;
    }

    public async Task<IEnumerable<Account>> GetAllProducts() => await Task.FromResult(_accounts);
}