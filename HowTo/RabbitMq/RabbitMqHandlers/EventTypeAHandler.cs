using Goober.RabbitMq.Abstractions;
using RabbitMq.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMq.RabbitMqHandlers
{
    public class EventTypeAHandler : IMessageHandler<EventTypeA>
    {
        public async Task ProcessAsync(EventTypeA @event)
        {
            Thread.Sleep(1000);
        }
    }
}
