namespace OrderService.Messaging
{
    public interface IMessagePublisher
    {
        public void Publish<T>(T message, string exchange);
    }
}
