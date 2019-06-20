using Goober.RabbitMq.Abstractions;
using System;
using System.Threading.Tasks;

namespace Goober.RabbitMq.Internal
{
    internal interface IMessageHandlerInvoker
    {
        Delegate CreateHandlerDelegate();
    }
}
