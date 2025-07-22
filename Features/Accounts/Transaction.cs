namespace Simple_Account_Service.Features.Accounts;

public class Transaction
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    public Guid? CounterpartyAccountId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public TransactionType Type { get; set; }

    public string? Description { get; set; }

    public DateTime Timestamp { get; set; }
}


public enum TransactionType
{
    Credit,
    Debit
}