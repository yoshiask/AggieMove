using AggieMove.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
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
            InsertIntoHistory(newGpsData);

            // Compute average speed
            var ticks = GpsHistory.Select(d => d.Date.UtcTicks);
            (long t_lat, double latSpeed) = GpsHistory.Select(d => d.Lat)
                .Zip(ticks, (l, t) => (t, l))
                .BackwardFiniteDifference()
                .AsParallel().Last();
            (long t_long, double longSpeed) = GpsHistory.Select(d => d.Long)
                .Zip(ticks, (l, t) => (t, l))
                .BackwardFiniteDifference()
                .AsParallel().Last();

            SynchronizationContext.Current.Post(_ =>
            {
                Speed = Math.Sqrt(latSpeed * latSpeed + longSpeed * longSpeed);
            }, null);
        }

        private void InsertIntoHistory(GpsData newData)
        {
            if (GpsHistory.Count == 10)
                GpsHistory.RemoveLast();

            GpsHistory.AddFirst(newData);
        }
    }
}
