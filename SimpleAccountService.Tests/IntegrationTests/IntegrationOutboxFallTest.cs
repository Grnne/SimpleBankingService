//using JetBrains.Annotations;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;
//using Simple_Account_Service.Application.Interfaces;
//using Simple_Account_Service.Features.Accounts.Entities;
//using Simple_Account_Service.Features.Transactions.Commands.TransferBetweenAccounts;
//using Simple_Account_Service.Features.Transactions.Entities;
//using Simple_Account_Service.Infrastructure.Data;
//using Simple_Account_Service.Infrastructure.Messaging.Outbox;
//using System.Net.Http.Json;
//using Xunit;

//namespace SimpleAccountService.Tests.IntegrationTests;

//[Collection("OutboxCollection")]
//[UsedImplicitly]
//public class OutboxTests(IntegrationTestWebAppFactory factory)
//    : IClassFixture<IntegrationTestWebAppFactory>, IDisposable
//{
//    private async Task CleanDatabaseAsync()
//    {
//        using var scope = factory.Services.CreateScope();
//        var context = scope.ServiceProvider.GetRequiredService<SasDbContext>();
//        context.Transactions.RemoveRange(context.Transactions);
//        context.Accounts.RemoveRange(context.Accounts);
//        await context.SaveChangesAsync();
//    }

//    public void Dispose()
//    {
//        GC.SuppressFinalize(this);
//    }

//    [Fact]
//    public async Task OutboxPublishesAfterRabbitMqRecovery()
//    {
//        //Arrange
//        using (var scope = factory.Services.CreateScope())
//        {
//            var context = scope.ServiceProvider.GetRequiredService<SasDbContext>();
//            context.OutboxMessages.RemoveRange(context.OutboxMessages);
//            await context.SaveChangesAsync();
//        }

//        await factory.StopRabbitMqAsync();

//        using (var scope = factory.Services.CreateScope())
//        {
//            var context = scope.ServiceProvider.GetRequiredService<SasDbContext>();

//            var outboxMessages = new[]
//            {
//                new OutboxMessage
//                {
//                    EventType = "MoneyDebited",
//                    Payload = "{\"Amount\":100,\"Currency\":\"USD\"}",
//                    CorrelationId = Guid.NewGuid(),
//                    CausationId = Guid.NewGuid(),
//                    OccurredAt = DateTime.UtcNow,
//                    Source = "TestSource",
//                    Version = "1.0"
//                },
//                new OutboxMessage
//                {
//                    EventType = "MoneyCredited",
//                    Payload = "{\"Amount\":100,\"Currency\":\"USD\"}",
//                    CorrelationId = Guid.NewGuid(),
//                    CausationId = Guid.NewGuid(),
//                    OccurredAt = DateTime.UtcNow,
//                    Source = "TestSource",
//                    Version = "1.0"
//                }
//            };

//            context.OutboxMessages.AddRange(outboxMessages);
//            await context.SaveChangesAsync();
//        }

//        using (var scope = factory.Services.CreateScope())
//        {
//            var context = scope.ServiceProvider.GetRequiredService<SasDbContext>();

//            var notProcessedMessages = await context.OutboxMessages
//                .Where(m => m.ProcessedAt == null)
//                .ToListAsync();
//            Assert.NotEmpty(notProcessedMessages);
//        }

//        await factory.StartRabbitMqAsync();

//        using (var scope = factory.Services.CreateScope())
//        {
//            var context = scope.ServiceProvider.GetRequiredService<SasDbContext>();

//            var processedMessages = await context.OutboxMessages
//                .Where(m => m.ProcessedAt != null)
//                .ToListAsync();
//            Assert.NotEmpty(processedMessages);

//            var notProcessedMessages = await context.OutboxMessages
//                .Where(m => m.ProcessedAt == null)
//                .ToListAsync();
//            Assert.Empty(notProcessedMessages);
//        }

//    }

//}
