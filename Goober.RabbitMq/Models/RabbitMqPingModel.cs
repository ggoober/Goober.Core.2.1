using System;
using System.Collections.Generic;
using System.Text;

namespace Goober.RabbitMq.Models
{
    public class RabbitMqPingModel
    {
        public DateTime? StartDateTime { get; set; }

        public DateTime? StopDateTime { get; set; }

        public bool IsRunning { get; set; }

        public int ServiceUpTime { get; set; }

        public List<MessageTypeConsumerPingModel> Consumers { get; set; }  = new List<MessageTypeConsumerPingModel>();
    }
}
