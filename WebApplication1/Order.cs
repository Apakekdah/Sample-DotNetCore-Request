using MassTransit;
using MessageContracts;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication1
{
    [Route("api/order")]
    public class Order : Controller
    {
        //readonly IRequestClient<SubmitOrder> _requestClient;
        private readonly IBus bus;

        //public Order(IRequestClient<SubmitOrder> requestClient)
        public Order(IBus bus)
        {
            //_requestClient = requestClient;
            this.bus = bus;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Submit(string id, CancellationToken cancellationToken)
        {
            try
            {
                //var result = await _requestClient.GetResponse<OrderAccepted>(new {OrderId = id}, cancellationToken);
                var result = await bus.Request<SubmitOrder, OrderAccepted>(new SubmitOrder { OrderId = id }, cancellationToken,
                    callback: cb =>
                    {
                        cb.SetRoutingKey("RouteKeyX");
                    });

                return Accepted(new { result.Message.OrderId });
            }
            catch (RequestTimeoutException)
            {
                return StatusCode((int)HttpStatusCode.RequestTimeout);
            }
        }
    }
}