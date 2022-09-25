using AggieMove.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using Esri.ArcGISRuntime.Geometry;
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
        public Mentor Mentor { get; }

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
        private double _speed;

        public VehicleViewModel(Mentor mentor)
        {
            Mentor = mentor;

            GpsHistory = new();
            GpsHistory.AddFirst(mentor.GPS);
        }

        public void UpdateSpeed(GpsData newGpsData)
        {
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
            Speed = latSpeed * TimeSpan.TicksPerHour;
        }

        private void InsertIntoHistory(GpsData newData)
        {
            if (GpsHistory.Count == 10)
                GpsHistory.RemoveLast();

            GpsHistory.AddFirst(newData);
        }
    }
}
