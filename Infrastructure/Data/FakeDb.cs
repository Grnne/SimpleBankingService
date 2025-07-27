using Simple_Account_Service.Application.ForFakesAndDummies;
using Simple_Account_Service.Features.Accounts.Entities;
using Simple_Account_Service.Features.Transactions.Entities;

namespace Simple_Account_Service.Infrastructure.Data;

public class FakeDb
{
    private readonly List<Owner> _users = [];
    private readonly List<Account> _accounts = [];
    private readonly List<Transaction> _transactions = [];
    private readonly List<Currency> _currencies = [];

    public FakeDb()
    {
        InitializeFakeData();
    }

    #region Account

    public Task<IEnumerable<Account>> GetAllAccountsAsync() =>
        Task.FromResult<IEnumerable<Account>>(_accounts);

    public Task<Account?> GetAccountByIdAsync(Guid id) =>
        Task.FromResult(_accounts.FirstOrDefault(a => a.Id == id));

    public Task<Account> AddAccountAsync(Account account)
    {
        _accounts.Add(account);

        return Task.FromResult(account);
    }

    public Task<bool> UpdateAccountAsync(Account updated)
    {
        var index = _accounts.FindIndex(a => a.Id == updated.Id);

        if (index < 0)
        {
            return Task.FromResult(false);
        }

        _accounts[index] = updated;

        return Task.FromResult(true);
    }

    public Task<bool> RemoveAccountAsync(Guid id)
    {
        var account = _accounts.FirstOrDefault(a => a.Id == id);

        if (account == null)
        {
            return Task.FromResult(false);
        }

        _accounts.Remove(account);

        return Task.FromResult(true);
    }

    #endregion

    #region Transaction

    public Task<Transaction?> GetTransactionByIdAsync(Guid id) =>
        Task.FromResult(_transactions.FirstOrDefault(t => t.Id == id));

    public Task<Transaction> AddTransactionAsync(Transaction transaction)
    {
        _transactions.Add(transaction);

        var account = _accounts.FirstOrDefault(a => a.Id == transaction.AccountId);

        account?.Transactions.Add(transaction);

        return Task.FromResult(transaction);
    }

    public Task<bool> UpdateTransactionAsync(Transaction updated)
    {
        var index = _transactions.FindIndex(t => t.Id == updated.Id);

        if (index < 0)
        {
            return Task.FromResult(false);
        }

        _transactions[index] = updated;

        var account = _accounts.FirstOrDefault(a => a.Id == updated.AccountId);

        if (account != null)
        {
            var txnIndex = account.Transactions.FindIndex(t => t.Id == updated.Id);

            if (txnIndex >= 0)
            {
                account.Transactions[txnIndex] = updated;
            }
        }

        return Task.FromResult(true);
    }

    public Task<bool> RemoveTransactionAsync(Guid id)
    {
        var transaction = _transactions.FirstOrDefault(t => t.Id == id);

        if (transaction == null)
        {
            return Task.FromResult(false);
        }

        _transactions.Remove(transaction);

        var account = _accounts.FirstOrDefault(a => a.Id == transaction.AccountId);

        var txnInAccount = account?.Transactions.FirstOrDefault(t => t.Id == id);

        if (txnInAccount != null)
        {
            account?.Transactions.Remove(txnInAccount);
        }

        return Task.FromResult(true);
    }

    #endregion

    #region Owner

    public Task<Owner?> GetOwnerByIdAsync(Guid id) =>
        Task.FromResult(_users.FirstOrDefault(o => o.Id == id));

    public Task<Owner> AddOwnerAsync(Owner owner)
    {
        _users.Add(owner);

        return Task.FromResult(owner);
    }

    public Task<bool> UpdateOwnerAsync(Owner updated)
    {
        var index = _users.FindIndex(o => o.Id == updated.Id);

        if (index < 0)
        {
            return Task.FromResult(false);
        }

        _users[index] = updated;

        return Task.FromResult(true);
    }

    public Task<bool> RemoveOwnerAsync(Guid id)
    {
        var owner = _users.FirstOrDefault(o => o.Id == id);

        if (owner == null)
        {
            return Task.FromResult(false);
        }

        _users.Remove(owner);

        return Task.FromResult(true);
    }

    #endregion

    #region Currency

    public Task<Currency?> GetCurrencyByCodeAsync(string code) =>
        Task.FromResult(_currencies.FirstOrDefault(c => c.Code == code));

    public Task<Currency> AddCurrencyAsync(Currency currency)
    {
        _currencies.Add(currency);

        return Task.FromResult(currency);
    }

    public Task<bool> UpdateCurrencyAsync(Currency updated)
    {
        var index = _currencies.FindIndex(c => c.Code == updated.Code);

        if (index < 0)
        {
            return Task.FromResult(false);
        }

        _currencies[index] = updated;

        return Task.FromResult(true);
    }

    public Task<bool> RemoveCurrencyAsync(string code)
    {
        var currency = _currencies.FirstOrDefault(c => c.Code == code);

        if (currency == null)
        {
            return Task.FromResult(false);
        }

        _currencies.Remove(currency);

        return Task.FromResult(true);
    }

    #endregion

    private void InitializeFakeData()
    {
        _users.AddRange(
        [
            new Owner { Id = Guid.NewGuid(), Name = "Иван" },
            new Owner { Id = Guid.NewGuid(), Name = "Анна" }
        ]);

        _currencies.AddRange(
        [
            new Currency { Code = "RUB", Name = "Российский рубль" },
            new Currency { Code = "USD", Name = "Доллар США" },
            new Currency { Code = "EUR", Name = "Евро" }
        ]);

        var account1 = new Account
        {
            Id = Guid.NewGuid(),
            OwnerId = _users[0].Id,
            Type = AccountType.Checking,
            Currency = "USD",
            Balance = 1500.00m,
            InterestRate = null,
            CreatedAt = DateTime.UtcNow.AddMonths(-4),
            ClosedAt = null,
            Transactions = []
        };

        var account2 = new Account
        {
            Id = Guid.NewGuid(),
            OwnerId = _users[0].Id,
            Type = AccountType.Deposit,
            Currency = "EUR",
            Balance = 5000.00m,
            InterestRate = 0.03m,
            CreatedAt = DateTime.UtcNow.AddMonths(-12),
            ClosedAt = null,
            Transactions = []
        };

        var account3 = new Account
        {
            Id = Guid.NewGuid(),
            OwnerId = _users[1].Id,
            Type = AccountType.Credit,
            Currency = "RUB",
            Balance = 2000.00m,
            InterestRate = 0.07m,
            CreatedAt = DateTime.UtcNow.AddMonths(-2),
            ClosedAt = null,
            Transactions = []
        };

        var account4 = new Account
        {
            Id = Guid.NewGuid(),
            OwnerId = _users[1].Id,
            Type = AccountType.Credit,
            Currency = "RUB",
            Balance = -2000.00m,
            InterestRate = 0.07m,
            CreatedAt = DateTime.UtcNow.AddMonths(-1),
            ClosedAt = null,
            Transactions = []
        };

        _accounts.AddRange([account1, account2, account3, account4]);

        var txn1 = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = account1.Id,
            CounterpartyAccountId = null,
            Amount = 1000.00m,
            Currency = "USD",
            Type = TransactionType.Credit,
            Description = "Initial deposit",
            Timestamp = DateTime.UtcNow.AddMonths(-3)
        };

        var txn2 = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = account1.Id,
            CounterpartyAccountId = account2.Id,
            Amount = 200.00m,
            Currency = "USD",
            Type = TransactionType.Debit,
            Description = "Transfer to EUR deposit",
            Timestamp = DateTime.UtcNow.AddMonths(-1)
        };

        var txn3 = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = account2.Id,
            CounterpartyAccountId = account1.Id,
            Amount = 200.00m,
            Currency = "EUR",
            Type = TransactionType.Credit,
            Description = "Received from USD checking account",
            Timestamp = DateTime.UtcNow.AddMonths(-1)
        };

        var txn4 = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = account3.Id,
            CounterpartyAccountId = null,
            Amount = 500.00m,
            Currency = "RUB",
            Type = TransactionType.Debit,
            Description = "Credit card payment",
            Timestamp = DateTime.UtcNow.AddMonths(-1)
        };

        _transactions.AddRange([txn1, txn2, txn3, txn4]);

        account1.Transactions.Add(txn1);
        account1.Transactions.Add(txn2);
        account2.Transactions.Add(txn3);
        account3.Transactions.Add(txn4);
    }
}