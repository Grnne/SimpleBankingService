using FluentValidation;
using JetBrains.Annotations;

namespace Simple_Account_Service.Features.Accounts.Queries.CheckAccountExists;

[UsedImplicitly]
public class CheckAccountExistsQueryValidator : AbstractValidator<CheckAccountExistsQuery>
{
    public CheckAccountExistsQueryValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("Идентификатор аккаунта обязателен и не может быть пустым.");
    }
}