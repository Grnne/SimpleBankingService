namespace SimpleAccountService.Tests.IntegrationTests;

[CollectionDefinition("TransactionsCollection")]
public class TransactionsCollection : ICollectionFixture<IntegrationTestWebAppFactory>;

[CollectionDefinition("OutboxCollection")]
public class OutboxCollection : ICollectionFixture<IntegrationTestWebAppFactory>;
