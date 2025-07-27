using FluentValidation;

namespace Simple_Account_Service.Features.Accounts.Commands.UpdateAccount;

public sealed class UpdateAccountCommandValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountCommandValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("Идентификатор аккаунта обязателен.");

        RuleFor(x => x.Request.InterestRate)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Request.InterestRate.HasValue)
            .WithMessage("Процентная ставка должна быть больше или равна 0.");

        RuleFor(x => x.Request.ClosedAt)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.Request.ClosedAt.HasValue)
            .WithMessage("Дата закрытия не может быть в будущем.");

        RuleFor(x => x)
            .Must(cmd => cmd.Request.InterestRate.HasValue || cmd.Request.ClosedAt.HasValue)
            .WithMessage("Должно быть заполнено хотя бы одно из полей: InterestRate или ClosedAt.");
    }
}