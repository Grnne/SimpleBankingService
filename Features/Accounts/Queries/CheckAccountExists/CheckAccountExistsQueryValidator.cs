using FluentValidation;

namespace Simple_Account_Service.Features.Accounts.Queries.CheckAccountExists;

public class CheckAccountExistsQueryValidator : AbstractValidator<CheckAccountExistsQuery>
{
    public CheckAccountExistsQueryValidator()
    {

    }
}