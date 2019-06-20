using Goober.RabbitMq.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Goober.RabbitMq.Services
{
    public interface IEventConsumer: IDisposable
    {
        void AddSubscription<TEvent, TEventHandler>()
            where TEvent : class
            where TEventHandler : class, IMessageHandler<TEvent>;

        void StartListening();
    }
}
