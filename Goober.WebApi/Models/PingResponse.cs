using System;
using System.Collections.Generic;
using System.Text;

namespace Hermes.WebApi.Models
{
    public class PingResponse
    {
        public bool IsSuccess { get; set; }

        public long TimeTakenMilliseconds { get; set; }
    }
}
