using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RealTimeIndexing.Services.RabbitMQ
{
    public class RabitMQProducer : IRabitMQProducer
    {
        public void SendIndexMessage<T>(T message)
        {
            //Here we specify the Rabbit MQ Server. we use rabbitmq docker image and use it
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
            };
            //Create the RabbitMQ connection using connection factory details as i mentioned above
            using var connection = factory.CreateConnection();
            //Here we create channel with session and model
            using var channel = connection.CreateModel();
            //declare the queue after mentioning name and a few property related to that
            channel.QueueDeclare("indexing", exclusive: false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, eventArgs) =>
            {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(message);
            };
            channel.BasicConsume(queue: "indexing", autoAck: true, consumer: consumer);
        }
    }
}
