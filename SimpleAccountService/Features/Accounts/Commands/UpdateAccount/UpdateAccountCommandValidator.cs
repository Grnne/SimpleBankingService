using FluentValidation;
using JetBrains.Annotations;

namespace Simple_Account_Service.Features.Accounts.Commands.UpdateAccount;

[UsedImplicitly]
public sealed class UpdateAccountCommandValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountCommandValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("Идентификатор аккаунта обязателен.");

        RuleFor(x => x.Request.InterestRate)
            .InclusiveBetween(0m, 1m).WithMessage("Процентная ставка должна быть от 0 до 1 (0%–100%).");

        RuleFor(x => x.Request.ClosedAt)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.Request.ClosedAt.HasValue)
            .WithMessage("Дата закрытия не может быть в будущем.");

        RuleFor(x => x)
            .Must(cmd => cmd.Request.InterestRate.HasValue || cmd.Request.ClosedAt.HasValue)
            .WithMessage("Должно быть заполнено хотя бы одно из полей: InterestRate или ClosedAt.");
    }
}