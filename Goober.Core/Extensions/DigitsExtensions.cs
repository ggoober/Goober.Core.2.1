using System;
using System.Globalization;

namespace Goober.Core.Extensions
{
    public static class DigitsExtensions
    {
        public static long? ToLong(this string value, NumberStyles style = NumberStyles.Integer, CultureInfo culture = null)
        {
            float ret;
            if (float.TryParse(value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out ret))
                return Convert.ToInt64(ret);

            return null;
        }

        public static decimal? ToDecimal(this string value, NumberStyles style = NumberStyles.Integer, CultureInfo culture = null)
        {
            if (value == null)
                return null;

            var ret = value.Replace(".", CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator)
                         .Replace(",", CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator);

            if (!decimal.TryParse(ret, style, culture ?? CultureInfo.InvariantCulture, out var result))
                return null;

            return result;
        }

        public static double? ToDouble(this string value, NumberStyles style = NumberStyles.Integer, CultureInfo culture = null)
        {
            if (value == null)
                return null;

            var ret = value.Replace(".", CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator)
                         .Replace(",", CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator);

            if (!double.TryParse(ret, style, culture ?? CultureInfo.InvariantCulture, out var result))
                return null;

            return result;
        }

        public static float? ToFloat(this string value, NumberStyles style = NumberStyles.Integer, CultureInfo culture = null)
        {
            if (value == null)
                return null;

            var ret = value.Replace(".", CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator)
                         .Replace(",", CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator);

            if (!float.TryParse(ret, style, culture ?? CultureInfo.InvariantCulture, out var result))
                return null;

            return result;
        }


        public static int? ToInt(this string value)
        {
            float ret;
            if (float.TryParse(value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out ret))
                return Convert.ToInt32(ret);

            return null;
        }
    }
}
