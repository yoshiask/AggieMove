using System;
using Windows.Devices.Geolocation;
using Windows.Foundation;

namespace AggieMove.Helpers
{
    public static class SpatialHelper
    {
        public static double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371; // Radius of the earth in km
            var dLat = ToRadians(lat2 - lat1);  // deg2rad below
            var dLon = ToRadians(lon2 - lon1);
            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c; // Distance in km
            return d;
        }

        public static double ToRadians(double deg)
        {
            return deg * (Math.PI / 180);
        }

        public static bool IsWithinRadius(double lat, double lon, double x, double y, double radius)
        {
            return GetDistance(lat, lon, x, y) <= radius;
        }

        public static Geolocator Geolocator { get; internal set; }
        private static Point CurrentLocationCache { get; set; }
        private static DateTime? CurrentLocationLastUpdated { get; set; }
        public static Point GetCachedLocation()
        {
            return CurrentLocationCache;
        }
        public async static System.Threading.Tasks.Task<Point> GetCurrentLocation(bool acceptCache = true)
        {
            if (CurrentLocationCache == null || !CurrentLocationLastUpdated.HasValue
                || DateTime.Now.Subtract(CurrentLocationLastUpdated.Value).TotalMinutes > 5
                || !acceptCache)
            {
                // Cache needs to be updated
                var accessStatus = await Geolocator.RequestAccessAsync();
                if (accessStatus == GeolocationAccessStatus.Allowed)
                {
                    double x, y;
                    Geolocator = new Geolocator { DesiredAccuracyInMeters = 1 };
                    Geoposition pos = await Geolocator.GetGeopositionAsync();
                    y = pos.Coordinate.Point.Position.Latitude;
                    x = pos.Coordinate.Point.Position.Longitude;
                    CurrentLocationCache = new Point(x, y);
                }
            }

            return CurrentLocationCache;
        }
    }
}
