using FluentValidation;

namespace Simple_Account_Service.Features.Transactions.Commands.CreateTransaction
{
    public sealed class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
    {
        public CreateTransactionCommandValidator()
        {
            RuleFor(x => x.AccountId)
                .NotEmpty().WithMessage("Идентификатор счета обязателен.");

            RuleFor(x => x.CreateTransactionDto)
                .NotNull().WithMessage("Данные транзакции обязательны.");

            RuleFor(x => x.CreateTransactionDto.Amount)
                .GreaterThan(0).WithMessage("Сумма транзакции должна быть больше 0.");

            RuleFor(x => x.CreateTransactionDto.Currency)
                .NotEmpty().WithMessage("Валюта обязательна для заполнения.")
                .Length(3).WithMessage("Код валюты должен содержать ровно 3 символа.");

            RuleFor(x => x.CreateTransactionDto.Type)
                .IsInEnum().WithMessage("Недопустимый тип транзакции.");

            RuleFor(x => x.CreateTransactionDto.Description)
                .MaximumLength(250).WithMessage("Описание транзакции не может превышать 250 символов.")
                .When(x => !string.IsNullOrEmpty(x.CreateTransactionDto.Description));
        }
    }
}
