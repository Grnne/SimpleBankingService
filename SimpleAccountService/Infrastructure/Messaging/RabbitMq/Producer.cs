using System.Text;
using RabbitMQ.Client;

namespace Simple_Account_Service.Infrastructure.Messaging.RabbitMq;

public class Producer
{

    public async Task Produce()
    {
        var factory = new ConnectionFactory { HostName = "rabbitmq" };
        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            "message",
            true,
            false,
            false,
            null);

        for (var i = 0; i < 10; i++)
        {
            var message = $"{DateTime.UtcNow} + {Guid.CreateVersion7()}";
            var body = Encoding.UTF8.GetBytes(message);
            
            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: "message",
                mandatory: true,
                basicProperties: new BasicProperties { Persistent = true },
                body: body);

            Console.WriteLine(message);

            await Task.Delay(2000);
        }

        
    }
}