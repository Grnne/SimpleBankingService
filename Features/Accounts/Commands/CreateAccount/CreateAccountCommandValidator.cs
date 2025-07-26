using FluentValidation;
using Simple_Account_Service.Features.Accounts.Entities;

namespace Simple_Account_Service.Features.Accounts.Commands.CreateAccount;

public sealed class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.Request.OwnerId)
            .NotEmpty().WithMessage("Идентификатор владельца обязателен.");

        RuleFor(x => x.Request.Type)
            .IsInEnum().WithMessage("Недопустимый тип счёта.");

        RuleFor(x => x.Request.Currency)
            .NotEmpty().WithMessage("Валюта обязательна для заполнения.")
            .Length(3).WithMessage("Код валюты должен содержать ровно 3 символа.");

        When(x => x.Request.Type is AccountType.Deposit or AccountType.Credit, () =>
        {
            RuleFor(x => x.Request.InterestRate)
                .NotNull().WithMessage("Процентная ставка обязательна для вкладов и кредитов.")
                .GreaterThanOrEqualTo(0).WithMessage("Процентная ставка не может быть отрицательной.");
        });

        When(x => x.Request.Type == AccountType.Checking, () =>
        {
            RuleFor(x => x.Request.InterestRate)
                .Null().WithMessage("Процентная ставка должна отсутствовать для текущих счетов.");
        });
    }
}