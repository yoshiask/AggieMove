using AggieMove.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Esri.ArcGISRuntime.UI;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using TamuBusFeed;
using TamuBusFeed.Models;

namespace AggieMove.ViewModels
{
    public partial class RouteViewModel : ObservableObject
    {
        private readonly SettingsService SettingsService = Ioc.Default.GetRequiredService<SettingsService>();

        public RouteViewModel()
        {
            LoadPatternsCommand = new AsyncRelayCommand(LoadPatternsAsync);
            LoadTimeTableCommand = new AsyncRelayCommand(LoadTimeTableAsync);

            PatternElements = new ObservableCollection<PatternElement>();
            Stops = new ObservableCollection<PatternElement>();
        }

        public RouteViewModel(Route route) : base()
        {
            SelectedRoute = route;
        }

        [ObservableProperty]
        private ObservableCollection<PatternElement> _patternElements;

        [ObservableProperty]
        private ObservableCollection<PatternElement> _stops;

        [ObservableProperty]
        private Route _selectedRoute;

        [ObservableProperty]
        private PatternElement _selectedPatternElement;

        [ObservableProperty]
        private TimeTable _timeTable;

        [ObservableProperty]
        private RouteVehiclesWatcher _routeVehiclesWatcher;

        [ObservableProperty]
        private Graphic _graphic;

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
            var patternElements = await SelectedRoute.GetDetailedPatternAsync(SettingsService.TargetDate);
            PatternElements = new ObservableCollection<PatternElement>(patternElements);
            Stops = new ObservableCollection<PatternElement>(SelectedRoute.Stops);
        }

        public async Task LoadTimeTableAsync()
        {
            TimeTable = await TamuBusFeedApi.GetTimetable(SelectedRoute.ShortName, SettingsService.TargetDate);
        }

        public void StartWatchingVehicles(NotifyCollectionChangedEventHandler vehiclesChangedHandler = null)
        {
            RouteVehiclesWatcher = new(this, vehiclesChangedHandler);
        }
    }
}
