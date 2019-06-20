using Goober.RabbitMq.Abstractions;
using RabbitMq.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMq
{
    public interface IEventTypeAHandler: IMessageHandler<EventTypeA>
    {
    }
}
