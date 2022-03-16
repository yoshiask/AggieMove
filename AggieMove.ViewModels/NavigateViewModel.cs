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
        private ObservableCollection<TravelMode> _travelModes;

        [System.ComponentModel.Bindable(true)]
        public ObservableCollection<TamuBusFeed.Models.SearchResult> Stops { get; set; } = new ObservableCollection<TamuBusFeed.Models.SearchResult>();

        [System.ComponentModel.Bindable(true)]
        public ObservableCollection<TamuBusFeed.Models.SearchResult> SearchResults { get; } = new ObservableCollection<TamuBusFeed.Models.SearchResult>();

        [System.ComponentModel.Bindable(true)]
        public ObservableCollection<Route> Routes { get; } = new ObservableCollection<Route>();

        public ObservableCollection<TravelMode> TravelModes
        {
            get => _travelModes;
            set => SetProperty(ref _travelModes, value);
        }

        private Route selectedRoute;
        [System.ComponentModel.Bindable(true)]
        public Route SelectedRoute
        {
            get => selectedRoute;
            set => SetProperty(ref selectedRoute, value);
        }

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
            TravelModes = new(_router.RouteTaskInfo.TravelModes);
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
            Routes.Clear();
            try
            {
                var routes = await TamuArcGisApi.SolveRoute(_router, Stops);
                foreach (Route r in routes)
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
            NavigationService.Navigate("RouteView", SelectedRoute);
        }
    }
}
