using Simple_Account_Service.Features.Accounts.Entities;

namespace Simple_Account_Service.Features.Accounts.Commands.CreateAccount;

public class CreateAccountDto
{
    public Guid OwnerId { get; set; }

    public AccountType Type { get; set; }

    public string Currency { get; set; } = null!;

    public decimal? InterestRate { get; set; }
}