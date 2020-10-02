using RabbitMQ.Client.Events;

namespace Model.Events
{
    public interface IEventHandler<T> where T : IEvent
    {
        void Handle(object sender, BasicDeliverEventArgs ea);
    }
}