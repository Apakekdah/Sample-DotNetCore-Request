using GreenPipes;
using MassTransit;
using MassTransit.RabbitMqTransport;
using MassTransit.RabbitMqTransport.Configuration;
using MassTransit.RabbitMqTransport.Topology.Settings;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using System.Threading;
using System.Threading.Tasks;

namespace Sample_RequestResponse
{
    public class MessageQueueService : BackgroundService
    {
        readonly IBusControl _bus;

        public MessageQueueService()
        {
            var name = "localhost";
            var host = "localhost";
            var port = 5672;
            var user = "";
            var password = "";
            var vhost = "mtb";
            var concurrent = 5;

            _bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(new RabbitMqHostAddress(host, port, vhost), c =>
                {
                    c.Username(user);
                    c.Password(password);
                });

                cfg.ReceiveEndpoint("order-service", e =>
                {
                    e.ConfigureConsumeTopology = false;

                    // This will help to bind Exchange-To-Queue with routekey
                    ((RabbitMqReceiveSettings)((RabbitMqReceiveEndpointConfiguration)e).Settings).RoutingKey = "RouteKeyX";

                    e.Durable = false;
                    e.AutoDelete = true;
                    e.Exclusive = false;
                    e.ExchangeType = ExchangeType.Direct;
                    e.ExclusiveConsumer = false;

                    //e.BindQueue = true;
                    e.Bind("os", ctx =>
                    {
                        // this routing only available for Exchange only :(
                        ctx.RoutingKey = "RouteKeyX";
                        ctx.ExchangeType = ExchangeType.Direct;
                        ctx.Durable = false;
                        ctx.AutoDelete = true;
                    });

                    e.Consumer(typeof(SubmitOrderConsumer), f => new SubmitOrderConsumer());

                    e.UseMessageRetry(c => c.None());

                    e.UseRetry(c => c.None());

                    //e.Handler<SubmitOrder>(context =>
                    //{
                    //    Console.WriteLine("Order: {0}", context.Message.OrderId);

                    //    return context.RespondAsync<OrderAccepted>(new
                    //    {
                    //        context.Message.OrderId
                    //    });
                    //});
                });

                cfg.AutoStart = true;
            });
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return _bus.StartAsync(stoppingToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.WhenAll(base.StopAsync(cancellationToken), _bus.StopAsync(cancellationToken));
        }
    }
}