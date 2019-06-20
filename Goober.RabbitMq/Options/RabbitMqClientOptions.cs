using System.Collections.Generic;

namespace Goober.RabbitMq.Options
{
    public class RabbitMqClientOptions
    {
        public string Host { get; set; }

        public ushort Port { get; set; } = 5672;

        public string VirtualHost { get; set; } = "/";

        public string UserName { get; set; }

        public string Password { get; set; }

        public string AppName { get; set; }

        public List<RabbitMqProducerOptions> Producers { get; set; } = new List<RabbitMqProducerOptions>();

        public List<RabbitMqConsumerOptions> Consumers { get; set; } = new List<RabbitMqConsumerOptions>();
    }
}
