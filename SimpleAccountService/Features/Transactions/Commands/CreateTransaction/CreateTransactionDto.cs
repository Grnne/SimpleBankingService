using JetBrains.Annotations;
using Simple_Account_Service.Features.Transactions.Entities;

namespace Simple_Account_Service.Features.Transactions.Commands.CreateTransaction;

[UsedImplicitly]
public class CreateTransactionDto
{
    [UsedImplicitly]

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public TransactionType Type { get; set; }

    public string? Description { get; set; }
}