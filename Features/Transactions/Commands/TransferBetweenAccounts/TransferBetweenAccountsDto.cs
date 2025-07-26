namespace Simple_Account_Service.Features.Transactions.Commands.TransferBetweenAccounts;

public class TransferBetweenAccountsDto
{
    public Guid DestinationAccountId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public string? Description { get; set; }
}
