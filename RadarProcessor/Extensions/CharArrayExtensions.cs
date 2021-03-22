using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

using RadarProcessor.Enums;

namespace RadarProcessor.Extensions
{
    public static class CharArrayExtensions
    {
        public static DateTime? ToDateTime(this char[] dateTime)
        {
            //Use DateTime.TryParse with custom IFormatter instead of the regex
            //var result = DateTime.ParseExact(new string(dateTime), "dd/mm/yyyyHH:mm:ss");
            var dateTimeString = new string(dateTime);
            var regex = new Regex(@"(\d{2})/(\d{2})/(\d{4})(\d{2}):(\d{2}):(\d{2})");

            var match = regex.Match(dateTimeString.Trim());

            if (!match.Success)
            {
                return null;
            }

            var year = int.Parse(match.Groups[3].Value);
            var month = int.Parse(match.Groups[2].Value);
            var day = int.Parse(match.Groups[1].Value);

            var hours = int.Parse(match.Groups[4].Value);
            var minutes = int.Parse(match.Groups[5].Value);
            var seconds = int.Parse(match.Groups[6].Value);

            return new DateTime(year, month, day, hours, minutes, seconds);
        }

        public static OperationType ToOperationType(this char operationTypeChar)
        {
            switch (operationTypeChar)
            {
                case 'O':
                    return OperationType.Overflight;
                case 'A':
                    return OperationType.Arrival;
                case 'D':
                    return OperationType.Departure;
                default:
                    return OperationType.Unknown;
            }
        }
    }
}