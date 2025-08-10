using Simple_Account_Service.Features.Accounts.Entities;
using System.Text.Json.Serialization;

namespace Simple_Account_Service.Features.Transactions.Entities;

public class Transaction
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    [JsonIgnore]
    public Account Account { get; set; } = null!;

    public Guid? CounterpartyAccountId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public TransactionType Type { get; set; }

    public string? Description { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public uint Version { get; set; }
}


public enum TransactionType
{
    Credit,
    Debit
}