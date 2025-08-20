using JetBrains.Annotations;
using Simple_Account_Service.Features.Accounts.Entities;
using Simple_Account_Service.Features.Transactions;

namespace Simple_Account_Service.Features.Accounts;

[UsedImplicitly]
public class AccountDto
{
    public Guid Id { get; set; }

    [UsedImplicitly]
    public Guid OwnerId { get; set; }

    [UsedImplicitly]
    public AccountType Type { get; set; }

    [UsedImplicitly]
    public string Currency { get; set; } = null!;

    [UsedImplicitly]
    public decimal Balance { get; set; }

    [UsedImplicitly]
    public decimal? CreditLimit { get; set; }

    [UsedImplicitly]
    public decimal? InterestRate { get; set; }

    [UsedImplicitly]
    public DateTime CreatedAt { get; set; }

    [UsedImplicitly]
    public DateTime? ClosedAt { get; set; }
    
    [UsedImplicitly]
    public bool Frozen { get; set; }

    [UsedImplicitly]
    public List<TransactionDto> Transactions { get; set; } = [];
}