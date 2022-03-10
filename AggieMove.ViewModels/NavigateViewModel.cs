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
using System.Text;
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
        }

        /// <summary>
        /// The <see cref="INavigationService"/> instance currently in use.
        /// </summary>
        private readonly INavigationService NavigationService = Ioc.Default.GetRequiredService<INavigationService>();

        [System.ComponentModel.Bindable(true)]
        public ObservableCollection<MapPoint> Stops { get; } = new ObservableCollection<MapPoint>();

        [System.ComponentModel.Bindable(true)]
        public ObservableCollection<Route> Routes { get; } = new ObservableCollection<Route>();

        private Route selectedRoute;
        [System.ComponentModel.Bindable(true)]
        public Route SelectedRoute
        {
            get => selectedRoute;
            set => SetProperty(ref selectedRoute, value);
        }

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

        public async Task AddStopAsync()
        {
            var result = await NavigationService.ShowDialog("Dialogs.AddPointPage", this);
            if (result.Button != DialogButtonResult.Secondary && result.Result is Feature feature)
            {
                Stops.Add(feature.Geometry.Extent.GetCenter());
            }
        }

        public async Task LoadRoutesAsync()
        {
            Routes.Clear();
            foreach (Route r in await TamuArcGisApi.SolveRoute(Stops))
            {
                Routes.Add(r);
            }
        }

        public void ViewRoute()
        {
            NavigationService.Navigate("RouteView", SelectedRoute);
        }
    }
}
