using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Model.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Model.BackgroundServices
{
    public class RabbitMQHostedService<TEvent, TEventHandler> : Microsoft.Extensions.Hosting.BackgroundService where TEvent : IEvent
                                                                                                               where TEventHandler : IEventHandler<TEvent>
    {
        private readonly IServiceProvider _serviceCollection;
        private readonly RabbitOption _options;

        private readonly ILogger _logger;

        private IConnection _connection;

        private IModel _channel;

        private string _queueName;

        public RabbitMQHostedService(IOptions<RabbitOption> options,
            ILoggerFactory loggerFactory,
            IServiceProvider serviceCollection)
        {
            _serviceCollection = serviceCollection;
            _options = options.Value;
            _logger = loggerFactory.CreateLogger<RabbitMQHostedService<TEvent, TEventHandler>>();
            ConsumeEvent();
        }

        private void ConsumeEvent()
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                UserName = _options.UserName,
                Password = _options.Password,
                Port = _options.Port,
                VirtualHost = _options.VHost,
            };

            // create connection
            _connection = factory.CreateConnection();

            // create channel
            _channel = _connection.CreateModel();

            var exchangeName = typeof(TEvent).FullName;

            _channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout);
            _queueName = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(_queueName, exchangeName, "");
            _channel.BasicQos(0, 1, false);

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);

            var eventHandler = _serviceCollection.GetService(typeof(TEventHandler));

            consumer.Received += ((IEventHandler<TEvent>)eventHandler).Handle;

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume(_queueName, false, consumer);
            return Task.CompletedTask;
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogWarning("Connection Shutdown");
        }

        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e)
        {
        }

        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e)
        {
        }

        private void OnConsumerRegistered(object sender, ConsumerEventArgs e)
        {
        }

        private void OnConsumerShutdown(object sender, ShutdownEventArgs e)
        {
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}