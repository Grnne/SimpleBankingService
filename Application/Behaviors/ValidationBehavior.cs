using FluentValidation;
using MediatR;

namespace Simple_Account_Service.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            validators.Select(v =>
                v.ValidateAsync(context, cancellationToken))).ConfigureAwait(false);

        var failures = validationResults
            .Where(r => r.Errors.Count > 0)
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return await next(cancellationToken).ConfigureAwait(false);
    }
}


// TODO maybe this stuff
//var errorsDictionary = _validators
//    .Select(x => x.Validate(context))
//    .SelectMany(x => x.Errors)
//    .Where(x => x != null)
//    .GroupBy(
//        x => x.PropertyName,
//        x => x.ErrorMessage,
//        (propertyName, errorMessages) => new
//        {
//            Key = propertyName,
//            Values = errorMessages.Distinct().ToArray()
//        })
//    .ToDictionary(x => x.Key, x => x.Values);