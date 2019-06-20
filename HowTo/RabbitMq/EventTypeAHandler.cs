using Goober.RabbitMq.Abstractions;
using RabbitMq.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMq
{
    public class EventTypeAHandler : IEventTypeAHandler
    {
        public async Task ProcessAsync(EventTypeA @event)
        {
        }
    }
}
