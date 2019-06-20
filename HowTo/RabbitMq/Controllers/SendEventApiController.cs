using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goober.RabbitMq.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMq.Models;

namespace RabbitMq.Controllers
{
    [ApiController]
    public class SendEventApiController : ControllerBase
    {
        private readonly IRabbitMqMessageProducer _eventProducer;

        public SendEventApiController(IRabbitMqMessageProducer eventProducer)
        {
            _eventProducer = eventProducer;
        }

        [Route("api/send-event/type-a")]
        [HttpPost]
        public void SentEventTypeA([FromBody]SendEventRequest request)
        {
            _eventProducer.Publish(new EventTypeA { MessageA = request.Messagge });
        }

        [Route("api/send-event/type-b")]
        [HttpPost]
        public void SentEventTypeB([FromBody]SendEventRequest request)
        {
            _eventProducer.Publish(new EventTypeB { MessageB = request.Messagge });
        }
    }
}