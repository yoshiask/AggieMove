using System;

namespace AggieMove.Converters
{
    public static class FormatConverters
    {
        public static string GetTravelModeDisplayName(string name) => name.Replace('_', ' ');

        public static string FormatDuration(TimeSpan duration)
        {
            string[] comps = new string[3];
            if (duration.Days > 0)
            {
                comps[0] = $"{duration.Days} day";
                if (duration.Days > 1)
                    comps[0] += "s";
            }
            if (duration.Hours > 0)
            {
                comps[1] = $"{duration.Hours} hr";
                if (duration.Hours > 1)
                    comps[1] += "s";
            }
            if (duration.Minutes > 0)
            {
                comps[2] = $"{duration.Minutes} min";
                if (duration.Minutes > 1)
                    comps[2] += "s";
            }

            return string.Join(" ", comps);
        }

        public static string FormatDistance(double meters)
        {
            // Convert to kilometers, then miles
            double miles = meters / 1000 * 0.6213711922;
            return $"{miles:N1} mi";
        }
    }
}
