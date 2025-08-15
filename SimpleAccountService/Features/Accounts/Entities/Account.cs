using Simple_Account_Service.Features.Transactions.Entities;

namespace Simple_Account_Service.Features.Accounts.Entities;

public class Account
{
    public Guid Id { get; set; }

    public Guid OwnerId { get; set; }

    public AccountType Type { get; set; }

    public string Currency { get; set; } = null!;

    public decimal Balance { get; set; }

    public decimal? CreditLimit { get; set; }

    public decimal? InterestRate { get; set; }

    public DateTime? LastInterestAccrualAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ClosedAt { get; set; }

    public List<Transaction> Transactions { get; set; } = [];

    public uint Version { get; set; }
}

public enum AccountType
{
    Checking,
    Deposit,
    Credit
}