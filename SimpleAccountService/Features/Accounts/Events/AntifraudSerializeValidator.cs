using FluentValidation;
using JetBrains.Annotations;
using Simple_Account_Service.Application.Abstractions;

namespace Simple_Account_Service.Features.Accounts.Events;

public class AntifraudEventValidator<T> : AbstractValidator<T> where T : AntifraudEvent
{
    public AntifraudEventValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("EventId обязателен")
            .Must(id => id != Guid.Empty).WithMessage("EventId не может быть пустым GUID");

        RuleFor(x => x.OccurredAt)
            .Must(d => d != default && d <= DateTime.UtcNow)
            .WithMessage("OccurredAt должен быть корректной датой в прошлом или настоящем");

        RuleFor(x => x.Meta).NotNull().WithMessage("Meta обязан быть задан");

        When(_ => true, () =>
        {
            RuleFor(x => x.Meta.Version)
                .Equal("v1").WithMessage("Version должен быть 'v1'");

            RuleFor(x => x.Meta.Source)
                .NotEmpty().WithMessage("Source обязателен");

            RuleFor(x => x.Meta.CorrelationId)
                .NotEmpty().WithMessage("CorrelationId обязателен")
                .Must(id => id != Guid.Empty).WithMessage("CorrelationId не может быть пустым GUID");

            RuleFor(x => x.Meta.CausationId)
                .NotEmpty().WithMessage("CausationId обязателен")
                .Must(id => id != Guid.Empty).WithMessage("CausationId не может быть пустым GUID");
        });
    }
}
[UsedImplicitly]
public class ClientBlockedValidator : AbstractValidator<ClientBlocked>
{
    public ClientBlockedValidator()
    {
        Include(new AntifraudEventValidator<ClientBlocked>());

        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("ClientId обязателен")
            .Must(id => id != Guid.Empty).WithMessage("ClientId не может быть пустым GUID");
    }
}
[UsedImplicitly]
public class ClientUnblockedValidator : AbstractValidator<ClientUnblocked>
{
    public ClientUnblockedValidator()
    {
        Include(new AntifraudEventValidator<ClientUnblocked>());

        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("ClientId обязателен")
            .Must(id => id != Guid.Empty).WithMessage("ClientId не может быть пустым GUID");
    }
}