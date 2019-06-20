using System;
using System.Threading.Tasks;

namespace Goober.RabbitMq.Services
{
    public interface IRabbitMqMessageProducer : IDisposable
    {
        Task PublishAsync<TMessage>(TMessage message) where TMessage : class;

        void Publish<TMessage>(TMessage message) where TMessage : class;
    }
}
