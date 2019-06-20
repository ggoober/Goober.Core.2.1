using System;

namespace Goober.RabbitMq.Options
{
    public class RabbitMqProducerOptions
    {
        public Type GetMessageType()
        {
            return Type.GetType(MessageTypeFullName);
        }

        /// <summary>
        /// Полное название типа сообщения
        /// </summary>
        public string MessageTypeFullName { get; set; }

        /// <summary>
        /// Количество попыток отправки конкретного сообщения в случае ошибки
        /// </summary>
        public uint PublishRetryCount { get; set; } = 10;

        /// <summary>
        /// Задержка между попытками отправить повторно конкретное сообщение в случае ошибки
        /// </summary>
        public uint PublishRetryIntervalInMilliseconds { get; set; } = 1000;
    }
}
