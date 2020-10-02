using System.Threading.Tasks;
using Model.Events;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;

namespace MicroService2.Handler
{
    public class EventHandler : IEventHandler<Event>
    {
        public async void Handle(object sender, BasicDeliverEventArgs ea)
        {
            var channel = ((EventingBasicConsumer)sender).Model;
            var content = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());

            var @event = JsonConvert.DeserializeObject(content, typeof(Event)) as Event;

            await HandleEvent(@event);

            channel.BasicAck(ea.DeliveryTag, false);
        }

        private Task HandleEvent(Event @event)
        {
            return Task.Run(() =>
            {
                //TODO: Manejo del Evento
            });
        }
    }
}