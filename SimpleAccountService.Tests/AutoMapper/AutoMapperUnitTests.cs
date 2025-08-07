using AutoMapper;
using Simple_Account_Service.Features.Accounts;
using Simple_Account_Service.Features.Accounts.Commands.CreateAccount;
using Simple_Account_Service.Features.Accounts.Commands.UpdateAccount;
using Simple_Account_Service.Features.Accounts.Entities;
using Simple_Account_Service.Features.Accounts.Queries.GetAccountStatement.Dto;
using Simple_Account_Service.Features.Transactions;
using Simple_Account_Service.Features.Transactions.Commands.CreateTransaction;
using Simple_Account_Service.Features.Transactions.Commands.TransferBetweenAccounts;
using Simple_Account_Service.Features.Transactions.Entities;

namespace SimpleAccountService.Tests.AutoMapper;

public class AutoMapperUnitTests
{
    private readonly MapperConfiguration _configuration;
    private readonly IMapper _mapper;

    public AutoMapperUnitTests()
    {
        _configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AccountsMappingProfile>();
            cfg.AddProfile<TransactionsMappingProfile>();
        });

        _mapper = _configuration.CreateMapper();
    }

    [Fact]
    public void AutoMapper_Configuration_IsValid()
    {
        // Act & Assert
        _configuration.AssertConfigurationIsValid();
    }

    [Fact]
    public void Transaction_To_TransactionDto_ValidInput_ValidResult()
    {
        // Arrange
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            CounterpartyAccountId = Guid.NewGuid(),
            Amount = 100.50m,
            Currency = "USD",
            Type = TransactionType.Credit,
            Description = "Test transaction",
            Timestamp = DateTime.UtcNow
        };

        // Act
        var dto = _mapper.Map<TransactionDto>(transaction);

        // Assert
        Assert.Equal(transaction.Id, dto.Id);
        Assert.Equal(transaction.AccountId, dto.AccountId);
        Assert.Equal(transaction.CounterpartyAccountId, dto.CounterpartyAccountId);
        Assert.Equal(transaction.Amount, dto.Amount);
        Assert.Equal(transaction.Currency, dto.Currency);
        Assert.Equal(transaction.Type, dto.Type);
        Assert.Equal(transaction.Description, dto.Description);
        Assert.Equal(transaction.Timestamp, dto.Timestamp);
    }

    [Fact]
    public void CreateTransactionDto_To_Transaction_ValidInput_ValidResult()
    {
        // Arrange
        var createDto = new CreateTransactionDto
        {
            Amount = 250.75m,
            Currency = "EUR",
            Type = TransactionType.Debit,
            Description = null
        };

        // Act
        var transaction = _mapper.Map<Transaction>(createDto);

        // Assert
        Assert.Equal(default, transaction.Id);
        Assert.Equal(default, transaction.AccountId);
        Assert.Null(transaction.CounterpartyAccountId);
        Assert.Equal(default, transaction.Timestamp);

        Assert.Equal(createDto.Amount, transaction.Amount);
        Assert.Equal(createDto.Currency, transaction.Currency);
        Assert.Equal(createDto.Type, transaction.Type);
        Assert.Null(transaction.Description);
    }

    [Fact]
    public void TransferDto_To_Transaction_ValidInput_ValidResult()
    {
        // Arrange
        var transferDto = new TransferDto
        {
            Amount = 123.45m,
            Currency = "GBP",
            Type = TransactionType.Credit,
            Description = "Transfer description",
            DestinationAccountId = Guid.NewGuid()
        };

        // Act
        var transaction = _mapper.Map<Transaction>(transferDto);

        // Assert
        Assert.Equal(transferDto.DestinationAccountId, transaction.CounterpartyAccountId);

        Assert.Equal(default, transaction.Id);
        Assert.Equal(default, transaction.AccountId);
        Assert.Equal(default, transaction.Timestamp);

        Assert.Equal(transferDto.Amount, transaction.Amount);
        Assert.Equal(transferDto.Currency, transaction.Currency);
        Assert.Equal(transferDto.Type, transaction.Type);
        Assert.Equal(transferDto.Description, transaction.Description);
    }

    [Fact]
    public void Account_To_AccountDto_ValidInput_ValidResult()
    {
        // Arrange
        var account = new Account
        {
            Id = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            Type = AccountType.Deposit,
            Currency = "USD",
            Balance = 1234.56m,
            InterestRate = 1.5m,
            CreatedAt = DateTime.UtcNow.AddMonths(-3),
            ClosedAt = null,
            Transactions =
            [
                new Transaction
                {
                    Id = Guid.NewGuid(), Amount = 100, Currency = "USD", Type = TransactionType.Credit,
                    Timestamp = DateTime.UtcNow
                }
            ]
        };

        // Act
        var dto = _mapper.Map<AccountDto>(account);

        // Assert
        Assert.Equal(account.Id, dto.Id);
        Assert.Equal(account.OwnerId, dto.OwnerId);
        Assert.Equal(account.Type, dto.Type);
        Assert.Equal(account.Currency, dto.Currency);
        Assert.Equal(account.Balance, dto.Balance);
        Assert.Equal(account.InterestRate, dto.InterestRate);
        Assert.Equal(account.CreatedAt, dto.CreatedAt);
        Assert.Equal(account.ClosedAt, dto.ClosedAt);
        Assert.Equal(account.Transactions.Count, dto.Transactions.Count);
    }

    [Fact]
    public void CreateAccountDto_To_Account_ValidInput_ValidResult()
    {
        // Arrange
        var createDto = new CreateAccountDto
        {
            OwnerId = Guid.NewGuid(),
            Type = AccountType.Checking,
            Currency = "EUR",
            InterestRate = 3.0m
        };

        // Act
        var account = _mapper.Map<Account>(createDto);

        // Assert
        Assert.Equal(default, account.Id);
        Assert.Equal(default, account.CreatedAt);
        Assert.Null(account.ClosedAt);
        Assert.NotNull(account.Transactions);
        Assert.Empty(account.Transactions);
        Assert.Equal(0, account.Balance); 

        Assert.Equal(createDto.OwnerId, account.OwnerId);
        Assert.Equal(createDto.Type, account.Type);
        Assert.Equal(createDto.Currency, account.Currency);
        Assert.Equal(createDto.InterestRate, account.InterestRate);
    }

    [Fact]
    public void UpdateAccountDto_To_Account_ValidInput_ValidResult()
    {
        // Arrange
        var updateDto = new UpdateAccountDto
        {
            InterestRate = 2.5m,
            ClosedAt = DateTime.UtcNow.Date
        };

        // Act
        var account = _mapper.Map<Account>(updateDto);

        // Assert
        Assert.Equal(default, account.Id);
        Assert.Equal(default, account.OwnerId);
        Assert.Equal(default, account.Type);
        Assert.Null(account.Currency);
        Assert.Equal(0m, account.Balance);
        Assert.Equal(default, account.CreatedAt);
        Assert.Empty(account.Transactions);

        Assert.Equal(updateDto.InterestRate, account.InterestRate);
        Assert.Equal(updateDto.ClosedAt, account.ClosedAt);
    }

    [Fact]
    public void Transaction_To_TransactionForStatementDto_ValidInput_ValidResult()
    {
        // Arrange
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            CounterpartyAccountId = Guid.NewGuid(),
            Amount = 999.99m,
            Type = TransactionType.Debit,
            Timestamp = DateTime.UtcNow
        };

        // Act
        var dto = _mapper.Map<TransactionForStatementDto>(transaction);

        // Assert
        Assert.Equal(transaction.Id, dto.Id);
        Assert.Equal(transaction.AccountId, dto.AccountId);
        Assert.Equal(transaction.CounterpartyAccountId, dto.CounterpartyAccountId);
        Assert.Equal(transaction.Amount, dto.Amount);
        Assert.Equal(transaction.Type, dto.Type);
        Assert.Equal(transaction.Timestamp, dto.Timestamp);
    }

}