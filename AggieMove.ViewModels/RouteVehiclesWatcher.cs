using AggieMove.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TamuBusFeed;

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
            var newMentors = await TamuBusFeedApi.GetVehicles(RouteShortName);

            // Construct a hash table to keep track of vehicles that have been updated
            List<string> updatedVehicles = new(Vehicles.Count);

            // Use the vehicle ID to match the previous vehicle object with the latest information
            foreach (var newMentor in newMentors)
            {
                var vehicle = Vehicles.FirstOrDefault(v => v.Mentor.Key == newMentor.Key);

                if (vehicle == null)
                {
                    // This vehicle has just been added to the list,
                    // create a new view model for it
                    vehicle = new(newMentor);
                    Vehicles.Add(vehicle);

                    updatedVehicles.Add(newMentor.Key);
                }
                else
                {
                    // This vehicle was already in service,
                    // update the previous entry
                    vehicle.UpdateSpeed(newMentor.GPS);

                    updatedVehicles.Add(newMentor.Key);
                }
            }

            // Remove any vehicles that weren't updated
            int i = 0;
            while (i < Vehicles.Count)
            {
                var vehicle = Vehicles[i];
                if (!updatedVehicles.Contains(vehicle.Mentor.Key))
                    Vehicles.RemoveAt(i);
                else
                    i++;
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
