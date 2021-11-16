using MassTransit;
using MessageContracts;
using System;
using System.Threading.Tasks;

namespace Sample_RequestResponse
{
    class SubmitOrderConsumer : IConsumer<SubmitOrder>
    {
        public async Task Consume(ConsumeContext<SubmitOrder> context)
        {
            var orderId = context.Message.OrderId;

            throw new ArgumentNullException("Test argument error");

            await context.RespondAsync(new OrderAccepted
            {
                OrderId = "Ok for : " + orderId
            });
        }
    }
}
