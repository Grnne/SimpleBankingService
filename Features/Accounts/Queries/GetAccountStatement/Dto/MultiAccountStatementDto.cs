namespace Simple_Account_Service.Features.Accounts.Queries.GetAccountStatement.Dto;

public class MultiAccountStatementDto
{
    public Guid OwnerId { get; set; }

    public DateTime PeriodStart { get; set; }

    public DateTime PeriodEnd { get; set; }

    public decimal TotalStartingBalance { get; set; }

    public decimal TotalEndingBalance { get; set; }

    public List<AccountStatementDto> AccountStatements { get; set; } = [];
}