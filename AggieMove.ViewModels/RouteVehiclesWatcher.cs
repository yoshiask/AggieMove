using AggieMove.Helpers;
using AggieMove.ViewModels;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
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
        private readonly TamuBusFeedApi Api = Ioc.Default.GetRequiredService<TamuBusFeedApi>();

        readonly Timer _timer;
        
        public RouteViewModel Route { get; }

        public ObservableCollection<VehicleViewModel> Vehicles { get; }

        public RouteVehiclesWatcher(RouteViewModel route, NotifyCollectionChangedEventHandler vehiclesChangedHandler = null, int interval = 10 * 1000)
        {
            Guard.IsNotNull(route);

            _timer = new Timer(async _ => await OnTick(), null, 0, interval);

            Route = route;
            Vehicles = new();

            if (vehiclesChangedHandler != null)
                Vehicles.CollectionChanged += vehiclesChangedHandler;
        }

        private async Task OnTick()
        {
            var newMentors = await Api.GetVehicles(Route.SelectedRoute.ShortName);

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
                    vehicle = new(newMentor, Route);
                    Vehicles.Add(vehicle);

                    updatedVehicles.Add(newMentor.Key);
                }
                else
                {
                    // This vehicle was already in service,
                    // update the previous entry
                    vehicle.UpdateMentor(newMentor);

                    updatedVehicles.Add(newMentor.Key);
                }
            }

            // Remove any vehicles that weren't updated
            Vehicles.RemoveAll(vehicle => !updatedVehicles.Contains(vehicle.Mentor.Key));
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
