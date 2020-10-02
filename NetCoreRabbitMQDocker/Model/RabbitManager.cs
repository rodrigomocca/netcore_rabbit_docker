using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using Model.Events;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Model
{
    public class RabbitManager : IRabbitManager
    {
        private readonly DefaultObjectPool<IModel> _objectPool;

        public RabbitManager(IPooledObjectPolicy<IModel> objectPolicy)
        {
            _objectPool = new DefaultObjectPool<IModel>(objectPolicy, Environment.ProcessorCount * 2);
        }

        public async Task Publish(IEvent @event)
        {
            if (@event == null)
            {
                return;
            }

            var channel = _objectPool.Get();

            var exchangeName = @event.GetType().FullName;

            try
            {
                channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout);

                var sendBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                await Task.Run(() =>
                {
                    channel.BasicPublish(exchangeName, "", properties, sendBytes);
                });
            }
            catch (Exception)
            {
                //TODO: Loguear el error pero no bloquear la operación
            }
            finally
            {
                _objectPool.Return(channel);
            }
        }
    }
}