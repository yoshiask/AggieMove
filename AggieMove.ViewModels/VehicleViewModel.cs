using AggieMove.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping.Popups;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using TamuBusFeed.Models;

namespace AggieMove.ViewModels
{
    public partial class VehicleViewModel : ObservableObject
    {
        /// <summary>
        /// Metadata returned from the bus feed API.
        /// </summary>
        public Mentor Mentor { get; private set; }

        /// <summary>
        /// The route this vehicle is servicing.
        /// </summary>
        public RouteViewModel Route { get; }

        /// <summary>
        /// A record of previous GPS data, used to compute speed
        /// and estimate arrival times.
        /// </summary>
        public LinkedList<GpsData> GpsHistory { get; }

        /// <summary>
        /// The map graphic representing this vehicle.
        /// </summary>
        public Graphic Graphic { get; set; }

        [ObservableProperty]
        private double? _speed;

        [ObservableProperty]
        private string _description;

        [ObservableProperty]
        private DateTimeOffset? _nextStopArrivalTime;

        public event Action<VehicleViewModel> MentorUpdated;

        public VehicleViewModel(Mentor mentor, RouteViewModel route)
        {
            Mentor = mentor;
            Route = route;

            GpsHistory = new();
            GpsHistory.AddFirst(mentor.GPS);

            UpdateDescription();
        }

        public void UpdateMentor(Mentor newMentor)
        {
            var newGpsData = newMentor.GPS;

            // Ignore duplicate data entries
            System.Diagnostics.Debug.WriteLine($"New: {newGpsData.Date}");
            System.Diagnostics.Debug.WriteLine($"Previous: {GpsHistory.First.Value.Date}");
            if (newGpsData.Date == GpsHistory.First.Value.Date)
                return;

            InsertIntoHistory(newGpsData);

            // Compute average speed
            var ticks = GpsHistory.Select(d => d.Date.UtcTicks);
            var vehicleMapPoints = GpsHistory.Select(d => new MapPoint(d.Long, d.Lat, TamuBusFeed.TamuArcGisApi.TamuSpatialReference));
            (long t_lat, double latSpeed) = vehicleMapPoints
                .Zip(ticks, (l, t) => (t, l))
                .BackwardFiniteDifference(
                    (left, right) => GeometryEngine.DistanceGeodetic(left, right, LinearUnits.Miles, null, GeodeticCurveType.Geodesic).Distance,
                    (dy, dx) => dy / dx)
                .AsParallel().Last();

            // Convert miles per tick to miles per hour
            Speed = Math.Abs(latSpeed * TimeSpan.TicksPerHour);

            // Estimate time arrival to next stop
            var nextMentorStop = newMentor.NextStops.FirstOrDefault();
            if (nextMentorStop != null)
            {
                const double tol = 20;
                var nextStop = Route.Stops.First(p => p.Key == nextMentorStop.Key);
                MapPoint nextStopPoint = new(nextStop.Longitude, nextStop.Latitude, TamuBusFeed.TamuArcGisApi.TamuSpatialReference);

                // Get location of vehicle and next stop along route
                var routeLine = Route.Graphic.Geometry as Polyline;
                double fracDst = GeometryEngine.FractionAlong(routeLine, nextStopPoint, tol);
                double fracVeh = GeometryEngine.FractionAlong(routeLine, vehicleMapPoints.First(), tol * 5);

                // Use fraction and route length to compute actual distance to stop
                double routeLength = GeometryEngine.LengthGeodetic(routeLine, LinearUnits.Miles);
                double distToDst = routeLength * Math.Min(fracDst - fracVeh, 1 - fracDst + fracVeh);

                // Divide distance by speed to get time
                double hrsToArrival = distToDst / Speed.Value;
                try
                {
                    if (!double.IsInfinity(hrsToArrival) && !double.IsNaN(hrsToArrival))
                        NextStopArrivalTime = DateTimeOffset.Now.AddHours(hrsToArrival);
                }
                catch (Exception ex)
                {

                }
            }

            // Update popup
            Mentor = newMentor;
            UpdateDescription();

            MentorUpdated?.Invoke(this);
        }

        private void UpdateDescription()
        {
            var apc = Mentor.APC;
            var nextStop = Mentor.NextStops.FirstOrDefault();

            const string newLine = "\r\n\r\n";
            StringBuilder desc = new(Mentor.Name + newLine);

            if (apc != null)
                desc.Append($"**{(double)apc.TotalPassenger / apc.PassengerCapacity:P0}** full{newLine}");

            if (nextStop != null)
                desc.Append($"Next stop: **{nextStop.Name}** ({nextStop.StopCode})\r\n");

            if (NextStopArrivalTime != null)
                desc.Append($"Arriving at {NextStopArrivalTime:t}, {NextStopArrivalTime.Value - DateTimeOffset.Now:hh\\:mm}\r\n");

            if (Speed != null)
                desc.Append($"Speed: {Speed:N0} mph{newLine}");

            Description = desc.ToString();
        }

        private void InsertIntoHistory(GpsData newData)
        {
            if (GpsHistory.Count == 10)
                GpsHistory.RemoveLast();

            GpsHistory.AddFirst(newData);
        }
    }
}
