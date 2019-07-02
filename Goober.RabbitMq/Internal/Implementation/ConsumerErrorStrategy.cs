using EasyNetQ;
using EasyNetQ.Consumer;
using System;

namespace Goober.RabbitMq.Internal.Implementation
{
    internal class ConsumerErrorStrategy : DefaultConsumerErrorStrategy
    {
        public ConsumerErrorStrategy(
                IConnectionFactory connectionFactory,
                ISerializer serializer,
                IConventions conventions,
                ITypeNameSerializer typeNameSerializer,
                IErrorMessageSerializer errorMessageSerializer)
            : base(connectionFactory, serializer, conventions, typeNameSerializer, errorMessageSerializer)
        {
        }

        public override AckStrategy HandleConsumerCancelled(ConsumerExecutionContext context)
        {
            return base.HandleConsumerCancelled(context);
        }

        public override AckStrategy HandleConsumerError(ConsumerExecutionContext context, Exception exception)
        {
            return AckStrategies.NackWithoutRequeue;
        }
    }
}
