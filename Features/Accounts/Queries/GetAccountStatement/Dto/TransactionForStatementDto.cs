using Simple_Account_Service.Features.Transactions.Entities;

namespace Simple_Account_Service.Features.Accounts.Queries.GetAccountStatement.Dto;

public class TransactionForStatementDto
{
    public Guid Id { get; set; }

    public Guid? CounterpartyAccountId { get; set; }

    public decimal Amount { get; set; }

    public TransactionType Type { get; set; }

    public DateTime Timestamp { get; set; }
}