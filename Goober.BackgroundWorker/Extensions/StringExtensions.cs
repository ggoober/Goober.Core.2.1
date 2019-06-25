using System;
using System.Globalization;

namespace Goober.BackgroundWorker.Extensions
{
    public static class StringExtensions
    {
        public static int? ToInt(this string value)
        {
            if (float.TryParse(value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out float ret))
                return Convert.ToInt32(ret);

            return null;
        }
    }
}
