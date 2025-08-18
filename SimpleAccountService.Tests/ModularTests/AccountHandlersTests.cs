//using AutoMapper;
//using JetBrains.Annotations;
//using MediatR;
//using Microsoft.Data.Sqlite;
//using Microsoft.EntityFrameworkCore;
//using Simple_Account_Service.Features.Accounts.Commands.CreateAccount;
//using Simple_Account_Service.Features.Accounts.Commands.DeleteAccount;
//using Simple_Account_Service.Features.Accounts.Commands.UpdateAccount;
//using Simple_Account_Service.Features.Accounts.Entities;
//using Simple_Account_Service.Features.Transactions.Entities;
//using Simple_Account_Service.Infrastructure.Data;
//using Simple_Account_Service.Infrastructure.Repositories;

//namespace SimpleAccountService.Tests.ModularTests;

//[UsedImplicitly] 
//public class AccountHandlersTests : IDisposable // TODO rework
//{
//    private readonly SqliteConnection _connection;
//    private readonly SasDbContext _context;
//    private readonly AccountRepository _repository;
//    private readonly IMapper _mapper;
//    private readonly IMediator _mediator;


//    public AccountHandlersTests(IMediator mediator)
//    {
//        _mediator = mediator;
//        _connection = new SqliteConnection("DataSource=:memory:");
//        _connection.Open();

//        var options = new DbContextOptionsBuilder<SasDbContext>()
//            .UseSqlite(_connection)
//            .Options;

//        _context = new TestSasDbContext(options);
//        _context.Database.OpenConnection();
//        _context.Database.EnsureCreated();

//        _repository = new AccountRepository(_context);

//        var mapperConfig = new MapperConfiguration(cfg =>
//        {
//            cfg.AddProfile<Simple_Account_Service.Features.Accounts.AccountsMappingProfile>();
//            cfg.AddProfile<Simple_Account_Service.Features.Transactions.TransactionsMappingProfile>();
//        });
//        _mapper = mapperConfig.CreateMapper();
//    }

//    [Fact]
//    [UsedImplicitly]
//    public async Task CreateAccountCommandHandler_ShouldCreateAccount()
//    {
//        var handler = new CreateAccountCommandHandler(_context, _repository, _mapper);

//        var createDto = new CreateAccountDto
//        {
//            OwnerId = Guid.NewGuid(),
//            Type = AccountType.Checking,
//            Currency = "USD",
//            InterestRate = 1.2m
//        };

//        var command = new CreateAccountCommand(createDto);

//        var result = await handler.Handle(command, CancellationToken.None);

//        Assert.True(result.Success);
//        Assert.NotNull(result.Response);
//        Assert.Equal(createDto.OwnerId, result.Response.OwnerId);
//        Assert.Equal(createDto.Type, result.Response.Type);
//        Assert.Equal(createDto.Currency, result.Response.Currency);
//        Assert.Equal(createDto.InterestRate, result.Response.InterestRate);
//        Assert.Equal(0, result.Response.Balance);
//    }

//    [Fact]
//    [UsedImplicitly]
//    public async Task DeleteAccountCommandHandler_ExistingAccount_ShouldDelete()
//    {
//        var account = new Account
//        {
//            Id = Guid.NewGuid(),
//            OwnerId = Guid.NewGuid(),
//            Type = AccountType.Checking,
//            Currency = "USD",
//            Balance = 100m,
//            CreatedAt = DateTime.UtcNow
//        };
//        _context.Accounts.Add(account);
//        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

//        var handler = new DeleteAccountCommandHandler(_repository);

//        var command = new DeleteAccountCommand(account.Id);

//        var result = await handler.Handle(command, CancellationToken.None);

//        Assert.True(result.Success);
//        Assert.True(result.Response);

//        var deleted = await _repository.GetByIdAsync(account.Id);
//        Assert.Null(deleted);
//    }

//    [Fact]
//    [UsedImplicitly]
//    public async Task DeleteAccountCommandHandler_NonExistingAccount_ShouldThrowNotFoundException()
//    {
//        var handler = new DeleteAccountCommandHandler(_repository);
//        var nonExistentId = Guid.NewGuid();

//        await Assert.ThrowsAsync<Simple_Account_Service.Application.Exceptions.NotFoundException>(async () =>
//        {
//            await handler.Handle(new DeleteAccountCommand(nonExistentId), CancellationToken.None);
//        });
//    }

//    [Fact]
//    [UsedImplicitly]
//    public async Task UpdateAccountCommandHandler_ValidUpdate_ShouldUpdateAccount()
//    {
//        var account = new Account
//        {
//            Id = Guid.NewGuid(),
//            OwnerId = Guid.NewGuid(),
//            Type = AccountType.Credit,
//            Currency = "EUR",
//            Balance = 500,
//            CreatedAt = DateTime.UtcNow
//        };
//        _context.Accounts.Add(account);
//        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

//        var handler = new UpdateAccountCommandHandler(_repository, _mapper);

//        var updateDto = new UpdateAccountDto
//        {
//            InterestRate = 2.5m,
//            CreditLimit = 1000m
//        };

//        var command = new UpdateAccountCommand(account.Id, updateDto);

//        var result = await handler.Handle(command, CancellationToken.None);

//        Assert.True(result.Success);
//        Assert.NotNull(result.Response);
//        Assert.Equal(updateDto.InterestRate, result.Response.InterestRate);
//        Assert.Equal(updateDto.CreditLimit, result.Response.CreditLimit);
//    }

//    [Fact]
//    [UsedImplicitly]
//    public async Task UpdateAccountCommandHandler_ClosedAccount_ShouldThrowConflictException()
//    {
//        var account = new Account
//        {
//            Id = Guid.NewGuid(),
//            OwnerId = Guid.NewGuid(),
//            Type = AccountType.Checking,
//            Currency = "EUR",
//            CreatedAt = DateTime.UtcNow,
//            ClosedAt = DateTime.UtcNow.AddDays(-1)
//        };
//        _context.Accounts.Add(account);
//        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

//        var handler = new UpdateAccountCommandHandler(_repository, _mapper);

//        var updateDto = new UpdateAccountDto
//        {
//            InterestRate = 3.0m
//        };
//        var command = new UpdateAccountCommand(account.Id, updateDto);

//        await Assert.ThrowsAsync<Simple_Account_Service.Application.Exceptions.ConflictException>(async () =>
//        {
//            await handler.Handle(command, CancellationToken.None);
//        });
//    }

//    public void Dispose()
//    {
//        _context.Dispose();
//        _connection.Close();
//        _connection.Dispose();

//        GC.SuppressFinalize(this);
//    }
//}


//public class TestSasDbContext(DbContextOptions<SasDbContext> options) : SasDbContext(options)
//{
//    protected override void OnModelCreating(ModelBuilder modelBuilder)
//    {
//        base.OnModelCreating(modelBuilder);

//        modelBuilder.Entity<Account>(entity =>
//        {
//            entity.Property(a => a.Version)
//                .IsRowVersion()
//                .HasDefaultValue(0);
//        });
//        modelBuilder.Entity<Transaction>(entity =>
//        {
//            entity.Property(a => a.Version)
//                .IsRowVersion()
//                .HasDefaultValue(0);
//        });
//    }
//}