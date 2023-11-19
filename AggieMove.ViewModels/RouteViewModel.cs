using AggieMove.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Esri.ArcGISRuntime.UI;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using TamuBusFeed;
using TamuBusFeed.Models;

namespace AggieMove.ViewModels
{
    public partial class RouteViewModel : ObservableObject
    {
        private readonly SettingsService Settings = Ioc.Default.GetRequiredService<SettingsService>();
        private readonly TamuBusFeedApi Api = Ioc.Default.GetRequiredService<TamuBusFeedApi>();

        public RouteViewModel()
        {
            LoadPatternsCommand = new AsyncRelayCommand(LoadPatternsAsync);
            LoadTimeTableCommand = new AsyncRelayCommand(LoadTimeTableAsync);
        }

        public RouteViewModel(RouteInfo route) : base()
        {
            SelectedRoute = route;
        }

        private bool _patternsLoaded = false;

        [ObservableProperty]
        private Pattern _pattern;

        [ObservableProperty]
        private List<PatternPoint> _stops;

        [ObservableProperty]
        private RouteInfo _selectedRoute;

        [ObservableProperty]
        private PatternPoint _selectedPatternPoint;

        [ObservableProperty]
        private List<TimeTable> _timeTables;

        [ObservableProperty]
        private RouteVehiclesWatcher _routeVehiclesWatcher;

        [ObservableProperty]
        private Graphic _graphic;

        [ObservableProperty]
        private string? _color;

        /// <summary>
        /// Gets the <see cref="IAsyncRelayCommand"/> instance responsible for loading patterns for the selected route.
        /// </summary>
        public IAsyncRelayCommand LoadPatternsCommand { get; }

        /// <summary>
        /// Gets the <see cref="IAsyncRelayCommand"/> instance responsible for loading the time table for the selected route.
        /// </summary>
        public IAsyncRelayCommand LoadTimeTableCommand { get; }

        /// <summary>
        /// Gets the <see cref="IAsyncRelayCommand"/> instance responsible for zooming to the selected stop.
        /// </summary>
        public IAsyncRelayCommand ZoomToStopCommand { get; }

        public async Task LoadPatternsAsync()
        {
            if (_patternsLoaded) return;

            Pattern = await Api.GetPatternPaths(SelectedRoute.Key);
            Color = Pattern.Color;

            Stops = Pattern.StopPoints().ToList();
            _patternsLoaded = true;
        }

        public async Task LoadTimeTableAsync()
        {
            TimeTables = await Api.GetTimetable(SelectedRoute.ShortName, Settings.TargetDate);
        }

        public void StartWatchingVehicles(NotifyCollectionChangedEventHandler vehiclesChangedHandler = null)
        {
            RouteVehiclesWatcher = new(this, vehiclesChangedHandler);
        }
    }
}
