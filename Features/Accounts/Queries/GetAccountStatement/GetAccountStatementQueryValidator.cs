using FluentValidation;

namespace Simple_Account_Service.Features.Accounts.Queries.GetAccountStatement;

public class CheckAccountExistsQueryValidator : AbstractValidator<GetAccountStatementQuery>
{
    public CheckAccountExistsQueryValidator()
    {
        RuleFor(x => x.OwnerId)
            .NotEmpty()
            .WithMessage("Идентификатор владельца не может быть пустым.");

        When(x => x.AccountId.HasValue, () =>
        {
            RuleFor(x => x.AccountId!.Value)
                .NotEmpty()
                .WithMessage("Идентификатор счета не может быть пустым, если указан.");
        });

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Дата начала периода не может быть пустой.");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage("Дата конца периода не может быть пустой.");

        RuleFor(x => x)
            .Must(x => x.StartDate <= x.EndDate)
            .WithMessage("Дата начала периода должна быть меньше или равна дате конца периода.");
    }
}