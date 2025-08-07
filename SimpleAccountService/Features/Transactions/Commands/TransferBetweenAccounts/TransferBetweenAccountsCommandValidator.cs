using FluentValidation;
using JetBrains.Annotations;
using Simple_Account_Service.Application.ForFakesAndDummies;

namespace Simple_Account_Service.Features.Transactions.Commands.TransferBetweenAccounts;

[UsedImplicitly]
public sealed class TransferBetweenAccountsCommandValidator : AbstractValidator<TransferBetweenAccountsCommand>
{
    public TransferBetweenAccountsCommandValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("Идентификатор исходного счёта обязателен.");

        RuleFor(x => x.TransferDto)
            .NotNull()
            .WithMessage("Данные перевода обязательны.");

        RuleFor(x => x.TransferDto.DestinationAccountId)
            .NotEmpty()
            .WithMessage("Идентификатор целевого счёта обязателен.")
            .Must((command, destId) => destId != command.AccountId)
            .WithMessage("Целевой счёт не должен совпадать с исходным.");

        RuleFor(x => x.TransferDto.Amount)
            .GreaterThan(0)
            .WithMessage("Сумма перевода должна быть больше 0.");

        RuleFor(x => x.TransferDto.Currency)
            .NotEmpty()
            .WithMessage("Валюта обязательна для заполнения.")
            .Must(Currency.IsSupported).WithMessage("Указана неподдерживаемая валюта.");

        RuleFor(x => x.TransferDto.Description)
            .MaximumLength(250)
            .When(x => !string.IsNullOrEmpty(x.TransferDto.Description))
            .WithMessage("Описание перевода не может превышать 250 символов.");
    }
}