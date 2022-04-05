using AggieMove.Helpers;
using AggieMove.Services;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TamuBusFeed;

namespace AggieMove.ViewModels
{
    public class NavigateViewModel : ObservableObject
    {
        public NavigateViewModel()
        {
            InitializeCommand = new AsyncRelayCommand(InitAsync);
            LoadRoutesCommand = new AsyncRelayCommand(LoadRoutesAsync);
            ViewRouteCommand = new RelayCommand(ViewRoute);
            AddStopCommand = new AsyncRelayCommand(AddStopAsync);
            SearchCommand = new AsyncRelayCommand<string>(SearchAsync);
        }

        /// <summary>
        /// The <see cref="INavigationService"/> instance currently in use.
        /// </summary>
        private readonly INavigationService NavigationService = Ioc.Default.GetRequiredService<INavigationService>();

        private RouteTask _router;
        private TravelMode _selectedTravelMode;

        [System.ComponentModel.Bindable(true)]
        public ObservableCollection<TamuBusFeed.Models.SearchResult> Stops { get; set; } = new ObservableCollection<TamuBusFeed.Models.SearchResult>();

        [System.ComponentModel.Bindable(true)]
        public ObservableCollection<TamuBusFeed.Models.SearchResult> SearchResults { get; } = new ObservableCollection<TamuBusFeed.Models.SearchResult>();

        [System.ComponentModel.Bindable(true)]
        public ObservableCollection<Route> Routes { get; } = new ObservableCollection<Route>();

        public ObservableCollection<TravelMode> TravelModes { get; } = new ObservableCollection<TravelMode>();

        private bool _routesLoaded = false;
        public bool RoutesLoaded
        {
            get => _routesLoaded;
            set => SetProperty(ref _routesLoaded, value);
        }

        private int _selectedRouteIndex;
        [System.ComponentModel.Bindable(true)]
        public int SelectedRouteIndex
        {
            get => _selectedRouteIndex;
            set => SetProperty(ref _selectedRouteIndex, value);
        }

        [System.ComponentModel.Bindable(true)]
        public TravelMode SelectedTravelMode
        {
            get => _selectedTravelMode;
            set => SetProperty(ref _selectedTravelMode, value);
        }

        public RouteResult Solve { get; private set; }

        [System.ComponentModel.Bindable(true)]
        /// <summary>
        /// Gets the <see cref="IAsyncRelayCommand"/> instance responsible for initialization.
        /// </summary>
        public IAsyncRelayCommand InitializeCommand { get; }

        [System.ComponentModel.Bindable(true)]
        /// <summary>
        /// Gets the <see cref="IAsyncRelayCommand"/> instance responsible for loading posts.
        /// </summary>
        public IAsyncRelayCommand LoadRoutesCommand { get; }

        [System.ComponentModel.Bindable(true)]
        /// <summary>
        /// Gets the <see cref="IAsyncRelayCommand"/> instance responsible for showing route details.
        /// </summary>
        public IRelayCommand ViewRouteCommand { get; }

        [System.ComponentModel.Bindable(true)]
        /// <summary>
        /// Gets the <see cref="IAsyncRelayCommand"/> instance responsible for adding stops.
        /// </summary>
        public IAsyncRelayCommand AddStopCommand { get; }

        [System.ComponentModel.Bindable(true)]
        /// <summary>
        /// Gets the <see cref="IAsyncRelayCommand"/> instance responsible for searching for stops.
        /// </summary>
        public IAsyncRelayCommand<string> SearchCommand { get; }

        public async Task InitAsync()
        {
            _router = await TamuArcGisApi.StartRouteTask();
            TravelModes.AddRange(_router.RouteTaskInfo.TravelModes);
        }

        public async Task AddStopAsync()
        {
            var result = await NavigationService.ShowDialog("Dialogs.AddPointPage", this);
            if (result.Button != DialogButtonResult.Secondary && result.Result is TamuBusFeed.Models.SearchResult searchResult)
            {
                Stops.Add(searchResult);
            }
        }

        public async Task SearchAsync(string query, CancellationToken token)
        {
            SearchResults.Clear();
            try
            {
                foreach (var result in await TamuArcGisApi.SearchAsync(query, token))
                {
                    SearchResults.Add(result);
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                SearchResults.Add(new TamuBusFeed.Models.SearchResult(ex.Message, TamuBusFeed.TamuArcGisApi.TamuCenter));
            }
        }

        public async Task LoadRoutesAsync()
        {
            try
            {
                Solve = await TamuArcGisApi.SolveRoute(_router, Stops, SelectedTravelMode);
                Routes.Clear();
                RoutesLoaded = true;
                foreach (Route r in Solve.Routes)
                {
                    Routes.Add(r);
                }
            }
            catch
            {
                throw;
            }
        }

        public void ViewRoute()
        {
            if (SelectedRouteIndex < 0)
                return;
            NavigationService.Navigate("DirectionsView", this);
        }
    }
}
