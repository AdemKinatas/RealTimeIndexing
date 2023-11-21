namespace RealTimeIndexing.Services.RabbitMQ
{
    public interface IRabitMQProducer
    {
        public void SendIndexMessage<T>(T message);
    }
}
