using MassTransit;
using MassTransit.RabbitMqTransport;
using MessageContracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using System;

namespace WebApplication1
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var name = "localhost";
            var host = "localhost";
            var port = 5672;
            var user = "";
            var password = "";
            var vhost = "mtb";
            var concurrent = 5;

            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(new RabbitMqHostAddress(host, port, vhost), c =>
                    {
                        c.Username(user);
                        c.Password(password);
                    });

                    cfg.Send<SubmitOrder>(c =>
                    {
                        c.UseRoutingKeyFormatter(ctx => ctx.RoutingKey());
                        c.UseCorrelationId(ctx => Guid.NewGuid());
                    });

                    cfg.Message<SubmitOrder>(c => c.SetEntityName("order-service"));

                    cfg.Publish<SubmitOrder>(c =>
                    {
                        c.ExchangeType = ExchangeType.Direct;
                        c.Durable = false;
                        c.AutoDelete = true;
                    });
                });

                var timeout = TimeSpan.FromSeconds(10);
                var serviceAddress = new Uri("rabbitmq://localhost/order-service");

                x.AddRequestClient<SubmitOrder>(serviceAddress, timeout);
            });
            services.AddMassTransitHostedService();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseEndpoints(x => x.MapControllers());
        }
    }
}