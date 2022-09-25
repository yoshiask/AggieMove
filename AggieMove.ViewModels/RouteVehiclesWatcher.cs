using AggieMove.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TamuBusFeed;
using TamuBusFeed.Models;

namespace AggieMove.Services
{
    public class RouteVehiclesWatcher : IDisposable
    {
        readonly Timer _timer;
        
        public string RouteShortName { get; }

        public ObservableCollection<VehicleViewModel> Vehicles { get; }

        public RouteVehiclesWatcher(string shortname, NotifyCollectionChangedEventHandler vehiclesChangedHandler = null, int interval = 10 * 10000)
        {
            _timer = new Timer(async _ => await OnTick(), null, 0, interval);

            RouteShortName = shortname;
            Vehicles = new();

            if (vehiclesChangedHandler != null)
                Vehicles.CollectionChanged += vehiclesChangedHandler;
        }

        private async Task OnTick()
        {
            var newVehicles = await TamuBusFeedApi.GetVehicles(RouteShortName);

            // Use the vehicle ID to match the previous vehicle object with the latest information
            // TODO: The resulting enumerable will not include vehicles that were added or removed from the list
            var prevCurPairs = newVehicles.Join(Vehicles, m => m.Key, v => v.Mentor.Key, (mentor, vehicle) => (vehicle, mentor));
            foreach (var (vehicle, mentor) in prevCurPairs)
            {
                vehicle.UpdateSpeed(mentor.GPS);
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
