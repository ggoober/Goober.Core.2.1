using System;

namespace Goober.RabbitMq.Options
{
    public class RabbitMqConsumerOptions
    {
        /// <summary>
        /// Полное название типа сообщения
        /// </summary>
        public string MessageTypeFullName { get; set; }

        /// <summary>
        /// Полное название сервиса, обрабатывающего сообщения
        /// </summary>
        public string HandlerTypeFullName { get; set; }

        private ushort _maxParallelHandlers = 1;
        /// <summary>
        /// Максимальное количество одновременно обрабатываемых сообщений.
        /// </summary>
        public ushort MaxParallelHandlers
        {
            get { return _maxParallelHandlers; }
            set
            {
                if (value < 1)
                    throw new ArgumentException("value must greater than 0");

                _maxParallelHandlers = value;
            }
        }

        private ushort _prefetchCount = 10;
        /// <summary>
        /// Максимальное количество предзагруженных сообщений из очереди.
        /// </summary>
        public ushort PrefetchCount
        {
            get { return _prefetchCount; }
            set
            {
                if (value < 1)
                    throw new ArgumentException("value must greater than 0");

                _prefetchCount = value;
            }
        }

        private ushort _messageProcessRetryCount = 3;
        /// <summary>
        /// Количество повторов обработки конкретного сообщения в случае возникнование ошибки
        /// </summary>
        public ushort MessageProcessRetryCount
        {
            get {
                return _messageProcessRetryCount;
            }
            set {
                if (value < 1)
                    throw new ArgumentException("value must greater than 0");

                _messageProcessRetryCount = value;
            }
        }

        /// <summary>
        ///  Интервал миллисекунд между попытками обработки конкретного сообщения в случаее возникновения ошибки
        /// </summary>
        public uint MessageProccessRetryDelayInMilliseconds { get; set; } = 1000;
    }
}
