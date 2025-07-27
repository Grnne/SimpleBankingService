using FluentValidation;

namespace Simple_Account_Service.Features.Accounts.Commands.DeleteAccount;

public sealed class DeleteAccountCommandValidator : AbstractValidator<DeleteAccountCommand>
{
    public DeleteAccountCommandValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("Идентификатор аккаунта обязателен и не может быть пустым.");
    }
}