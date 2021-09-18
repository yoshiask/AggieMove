using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TamuBusFeed;
using TamuBusFeed.Models;

namespace AggieMove.ViewModels
{
    public class RouteViewModel : ObservableRecipient
    {
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

        private ObservableCollection<PatternElement> _PatternElements;
        public ObservableCollection<PatternElement> PatternElements
        {
            get => _PatternElements;
            set => SetProperty(ref _PatternElements, value);
        }

        private ObservableCollection<PatternElement> _Stops;
        public ObservableCollection<PatternElement> Stops
        {
            get => _Stops;
            set => SetProperty(ref _Stops, value);
        }

        private Route _SelectedRoute;
        public Route SelectedRoute
        {
            get => _SelectedRoute;
            set => SetProperty(ref _SelectedRoute, value);
        }
        
        private PatternElement _SelectedPatternElement;
        public PatternElement SelectedPatternElement
        {
            get => _SelectedPatternElement;
            set => SetProperty(ref _SelectedPatternElement, value);
        }

        private TimeTable _TimeTable;
        public TimeTable TimeTable
        {
            get => _TimeTable;
            set => SetProperty(ref _TimeTable, value);
        }

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
            PatternElements = new ObservableCollection<PatternElement>(await SelectedRoute.GetDetailedPatternAsync());
            Stops = new ObservableCollection<PatternElement>(SelectedRoute.Stops);
        }

        public async Task LoadTimeTableAsync()
        {
            TimeTable = await TamuBusFeedApi.GetTimetable(SelectedRoute.ShortName);
        }
    }
}
