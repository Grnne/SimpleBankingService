using Simple_Account_Service.Features.Accounts.Entities;

namespace Simple_Account_Service.Features.Accounts.Queries.GetAccountStatement.Dto;

public class AccountStatementDto
{
    public Guid Id { get; set; }

    public AccountType Type { get; set; }

    public string Currency { get; set; } = null!;

    public decimal StartingBalance { get; set; }

    public decimal EndingBalance { get; set; }

    public decimal? InterestRate { get; set; }

    public List<TransactionForStatementDto> Transactions { get; set; } = [];
}