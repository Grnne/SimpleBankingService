using FluentValidation;
using JetBrains.Annotations;

namespace Simple_Account_Service.Features.Accounts.Commands.DeleteAccount;

[UsedImplicitly]
public sealed class DeleteAccountCommandValidator : AbstractValidator<DeleteAccountCommand>
{
    public DeleteAccountCommandValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("Идентификатор аккаунта обязателен и не может быть пустым.");
    }
}