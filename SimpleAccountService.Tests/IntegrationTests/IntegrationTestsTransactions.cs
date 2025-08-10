using Microsoft.Extensions.DependencyInjection;
using Simple_Account_Service.Features.Accounts.Entities;
using Simple_Account_Service.Features.Transactions.Commands.TransferBetweenAccounts;
using Simple_Account_Service.Features.Transactions.Entities;
using Simple_Account_Service.Infrastructure.Data;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using JetBrains.Annotations;

namespace SimpleAccountService.Tests.IntegrationTests;

[UsedImplicitly]
public class TransactionsControllerTests : IClassFixture<IntegrationTestWebAppFactory>, IDisposable
{
    private readonly HttpClient _client;
    private readonly IServiceScope _scope;
    private readonly SasDbContext _context;

    public TransactionsControllerTests(IntegrationTestWebAppFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");
        _scope = factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<SasDbContext>();

        CleanDatabase();
    }

    public void Dispose()
    {
        _scope.Dispose();

        GC.SuppressFinalize(this);
    }

    private void CleanDatabase()
    {
        _context.Transactions.RemoveRange(_context.Transactions);
        _context.Accounts.RemoveRange(_context.Accounts);
        _context.SaveChanges();
    }

    [Fact]
    [UsedImplicitly]
    public async Task ParallelTransferBetweenAccounts_50ValidCommands_CorrectBalances()
    {
        // Arrange
        const int count = 50;
        const decimal amount = 1000m;

        var sourceAccount = new Account
        {
            Id = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            Type = AccountType.Checking,
            Currency = "USD",
            Balance = count * amount,
            CreatedAt = DateTime.UtcNow
        };

        var destinationAccount = new Account
        {
            Id = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            Type = AccountType.Checking,
            Currency = "USD",
            Balance = 0m,
            CreatedAt = DateTime.UtcNow
        };

        _context.Accounts.Add(sourceAccount);
        _context.Accounts.Add(destinationAccount);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var transferDto = new TransferDto
        {
            DestinationAccountId = destinationAccount.Id,
            Amount = amount,
            Currency = "USD",
            Type = TransactionType.Debit,
            Description = "Test parallel transfer"
        };

        // Act
        var tasks = Enumerable.Range(0, count).Select(_ =>
            _client.PostAsync($"/api/Transactions/TransferBetweenAccounts/{sourceAccount.Id}", JsonContent.Create(transferDto))
        ).ToArray();

        await Task.WhenAll(tasks);

        // Обновил кеш EF core
        _context.ChangeTracker.Clear();

        var updatedSource =  await _context.Accounts.FindAsync(sourceAccount.Id);
        var updatedDestination = await _context.Accounts.FindAsync(destinationAccount.Id);

        // Assert
        // Насколько я понял, 1 трансфер должен быть успешным, остальные вернут ошибку
        Assert.NotNull(updatedSource);
        Assert.NotNull(updatedDestination);
        Assert.Equal(count * amount, updatedDestination.Balance + updatedSource.Balance);
        Assert.True(updatedDestination.Balance == amount);
    }
}