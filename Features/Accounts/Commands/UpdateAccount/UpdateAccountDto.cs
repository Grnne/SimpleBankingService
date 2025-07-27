namespace Simple_Account_Service.Features.Accounts.Commands.UpdateAccount;

public class UpdateAccountDto
{
    public decimal? InterestRate { get; set; }

    public DateTime? ClosedAt { get; set; }
}