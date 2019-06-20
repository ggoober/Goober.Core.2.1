using Goober.RabbitMq.BackgroundWorkers;
using Goober.RabbitMq.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

namespace Goober.RabbitMq.Controllers
{
    public class RabbitMqApiController: Controller
    {
        private readonly IServiceProvider _serviceProvider;

        public RabbitMqApiController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [HttpGet]
        [Route("api/rabbitmq/ping")]
        public RabbitMqPingModel Ping()
        {
            var ret = new RabbitMqPingModel();

            var rabbitMqWorker = (RabbitMqConsumerBackgroundWorker) _serviceProvider.GetServices<IHostedService>().FirstOrDefault(x=>x.GetType() == typeof(RabbitMqConsumerBackgroundWorker));

            if (rabbitMqWorker == null)
                return ret;

            ret.IsRunning = rabbitMqWorker.IsRunning;
            ret.ServiceUpTime = Convert.ToInt32(rabbitMqWorker.ServiceUpTime.TotalSeconds);
            ret.StartDateTime = rabbitMqWorker.StartDateTime;
            ret.StopDateTime = rabbitMqWorker.StopDateTime;

            var consumers = rabbitMqWorker.GetConsumers();

            foreach (var iConsumer in consumers)
            {
                if (iConsumer.Value.MessageHandlerInvoker == null)
                    continue;

                var handler = iConsumer.Value.MessageHandlerInvoker;
                
                var rec = new MessageTypeConsumerPingModel
                {
                    MessageTypeFullName = iConsumer.Key.FullName,
                    HandlerTypeFullName = handler.GetHandlerType().FullName,
                    AvgMessagesProcessingDurationInMilliseconds = handler.AvgMessagesProcessingDurationInMilliseconds,
                    LastMessageProcessingAttemptDate = handler.LastMessageProcessingAttemptDate,
                    LastMessageSuccessProcessedDate = handler.LastMessageSuccessProcessedDate,
                    LastMessageSuccessProcessedDurationInMilliseconds = handler.LastMessageSuccessProcessedDurationInMilliseconds,
                    MessagesErrorProcessedCount = handler.MessagesErrorProcessedCount,
                    MessagesProcessingAttemptsCount = handler.MessagesProcessingAttemptsCount,
                    MessagesSuccessProcessedCount = handler.MessagesSuccessProcessedCount,
                    MaxParallelHandlers = iConsumer.Value.MaxParallelHandlers,
                    MessageProccessRetryDelayInMilliseconds = iConsumer.Value.MessageProccessRetryDelayInMilliseconds,
                    MessageProcessRetryCount = iConsumer.Value.MessageProcessRetryCount,
                    PrefetchCount = iConsumer.Value.PrefetchCount
                };

                ret.Consumers.Add(rec);
            }

            return ret; ;
        }
    }
}
