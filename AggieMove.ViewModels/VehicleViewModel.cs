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

        public VehicleViewModel(Mentor mentor)
        {
            Mentor = mentor;

            GpsHistory = new();
            GpsHistory.AddFirst(mentor.GPS);

            UpdateDescription();
        }

        public void UpdateMentor(Mentor newMentor)
        {
            var newGpsData = newMentor.GPS;

            // Ignore duplicate data entries
            if (newGpsData.Date == GpsHistory.First.Value.Date)
                return;

            InsertIntoHistory(newGpsData);

            // Compute average speed
            var ticks = GpsHistory.Select(d => d.Date.UtcTicks);

            (long t_lat, double latSpeed) = GpsHistory.Select(d => new MapPoint(d.Long, d.Lat, TamuBusFeed.TamuArcGisApi.TamuSpatialReference))
                .Zip(ticks, (l, t) => (t, l))
                .BackwardFiniteDifference(
                    (left, right) => GeometryEngine.DistanceGeodetic(left, right, LinearUnits.Miles, null, GeodeticCurveType.Geodesic).Distance,
                    (dy, dx) => dy / dx)
                .AsParallel().Last();

            // Convert miles per tick to miles per hour
            Speed = Math.Abs(latSpeed * TimeSpan.TicksPerHour);

            // Update popup
            Mentor = newMentor;
            UpdateDescription();
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
                desc.Append($"Next stop: **{nextStop.Name}** ({nextStop.StopCode}){newLine}");

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
