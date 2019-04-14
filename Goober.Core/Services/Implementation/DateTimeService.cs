using System;

namespace Goober.Core.Services.Implementation
{
    internal class DateTimeService : IDateTimeService
    {
        public DateTime GetDateTimeNow()
        {
            return DateTime.Now;
        }
    }
}
