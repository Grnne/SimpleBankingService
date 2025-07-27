using JetBrains.Annotations;
using Simple_Account_Service.Features.Transactions.Entities;

namespace Simple_Account_Service.Features.Accounts.Queries.GetAccountStatement.Dto;

[UsedImplicitly]
public class TransactionForStatementDto
{
    [UsedImplicitly]
    public Guid Id { get; set; }

    [UsedImplicitly]
    public Guid AccountId { get; set; }

    [UsedImplicitly]
    public Guid? CounterpartyAccountId { get; set; }

    [UsedImplicitly]
    public decimal Amount { get; set; }

    [UsedImplicitly]
    public TransactionType Type { get; set; }

    [UsedImplicitly]
    public DateTime Timestamp { get; set; }
}