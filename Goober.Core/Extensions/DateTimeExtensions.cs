using Goober.Core.Services;
using Goober.Core.Services.Implementation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Linq;

namespace Goober.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime ToDateTime(this string value)
        {
            var parseResult = DateTime.Parse(s: value,
                provider: null,
                styles: DateTimeStyles.RoundtripKind);

            return parseResult;
        }

        public static void RegisterDateTimeService(this IServiceCollection services)
        {
            services.AddSingleton<IDateTimeService, DateTimeService>();
        }
        
        public static DateTime? ToDateTimeApproximately(this string value, DateTime currentDateTime, int maxDeltaInHours, out bool wasOutOfDelta)
        {
            wasOutOfDelta = false;

            if (string.IsNullOrEmpty(value) == true)
                return null;

            foreach (var iStrCulture in Cultures)
            {
                var culture = new CultureInfo(iStrCulture);
                var formats = GetDateParseExactFormats(culture);

                foreach (var iDateFormat in formats)
                {
                    try
                    {
                        DateTime dt = DateTime.ParseExact(value, iDateFormat, culture);

                        if (Math.Abs((currentDateTime - dt).TotalHours) < maxDeltaInHours)
                        {
                            return dt;
                        }
                        else
                        {
                            wasOutOfDelta = true;
                        }

                    }
                    catch { }
                }
            }


            return null;
        }
        
        private static string[] Cultures = new string[] { "ru-RU", "en-US" };

        private static string[] GetDateParseExactFormats(CultureInfo ci)
        {
            return new[] { "MM-dd-yyyy", "M-d-yyyy", "MM-d-yyyy", "M-dd-yyyy", "MM-dd-yy", "M-dd-yy", "MM-d-yy", "d-M-yy",
               "dd-MM-yyyy", "d-M-yyyy", "d-MM-yyyy", "dd-M-yyyy", "dd-MM-yy", "d-MM-yy", "dd-M-yy", "M-d-yy",
               "yyyy-dd-MM", "yyyy-d-M", "yyyy-d-MM", "yyyy-dd-M", "yy-dd-MM", "yy-d-M", "yy-d-MM", "yy-dd-M",
               "yyyy-MM-dd", "yyyy-M-d", "yyyy-MM-d", "yyyy-M-dd", "yy-MM-dd", "yy-M-d", "yy-MM-d", "yy-M-dd",

               "MM/dd/yyyy", "M/d/yyyy", "MM/d/yyyy", "M/dd/yyyy", "MM/dd/yy", "M/dd/yy", "MM/d/yy", "d/M/yy",
               "dd/MM/yyyy", "d/M/yyyy", "d/MM/yyyy", "dd/M/yyyy", "dd/MM/yy", "d/MM/yy", "dd/M/yy", "M/d/yy",
               "yyyy/dd/MM", "yyyy/d/M", "yyyy/d/MM", "yyyy/dd/M", "yy/dd/MM", "yy/d/M", "yy/d/MM", "yy/dd/M",
               "yyyy/MM/dd", "yyyy/M/d", "yyyy/MM/d", "yyyy/M/dd", "yy/MM/dd", "yy/M/d", "yy/MM/d", "yy/M/dd",

               "MM.dd.yyyy", "M.d.yyyy", "MM.d.yyyy", "M.dd.yyyy", "MM.dd.yy", "M.dd.yy", "MM.d.yy", "d.M.yy",
               "dd.MM.yyyy", "d.M.yyyy", "d.MM.yyyy", "dd.M.yyyy", "dd.MM.yy", "d.MM.yy", "dd.M.yy", "M.d.yy",
               "yyyy.dd.MM", "yyyy.d.M", "yyyy.d.MM", "yyyy.dd.M", "yy.dd.MM", "yy.d.M", "yy.d.MM", "yy.dd.M",
               "yyyy.MM.dd", "yyyy.M.d", "yyyy.MM.d", "yyyy.M.dd", "yy.MM.dd", "yy.M.d", "yy.MM.d", "yy.M.dd",

               "dd MMM yyyy", "dd MMMM yyyy", "d MMM yyyy", "d MMMM yyyy", "dd MMM yy", "dd MMMM yy", "d MMM yy", "d MMMM yy",
               "yyyy dd MMM", "yyyy dd MMMM", "yyyy d MMM", "yyyy d MMMM", "yy dd MMM", "yy dd MMMM", "yy d MMM", "yy d MMMM",

               "MMM dd yyyy", "MMMM dd yyyy", "MMM d yyyy", "MMMM d yyyy", "MMM dd yy", "MMMM dd yy", "MMM d yy", "MMMM d yy",
               "yyyy MMM dd", "yyyy MMMM dd", "yyyy MMM d", "yyyy MMMM d", "yy MMM dd", "yy MMMM dd", "yy MMM d", "yy MMMM d",

               "dd-MMM-yyyy", "dd-MMMM-yyyy", "d-MMM-yyyy", "d-MMMM-yyyy", "dd-MMM-yy", "dd-MMMM-yy", "d-MMM-yy", "d-MMMM-yy",
               "yyyy-dd-MMM", "yyyy-dd-MMMM", "yyyy-d-MMM", "yyyy-d-MMMM", "yy-dd-MMM", "yy-dd-MMMM", "yy-d-MMM", "yy-d-MMMM",

               "MMM-dd-yyyy", "MMMM-dd-yyyy", "MMM-d-yyyy", "MMMM-d-yyyy", "MMM-dd-yy", "MMMM-dd-yy", "MMM-d-yy", "MMMM-d-yy",
               "yyyy-MMM-dd", "yyyy-MMMM-dd", "yyyy-MMM-d", "yyyy-MMMM-d", "yy-MMM-dd", "yy-MMMM-dd", "yy-MMM-d", "yy-MMMM-d",
           }.Union(ci.DateTimeFormat.GetAllDateTimePatterns()).ToArray();
        }
    }
}
