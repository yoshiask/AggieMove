using AggieMove.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TamuBusFeed;
using TamuBusFeed.Models;

namespace AggieMove.ViewModels
{
    public class ExploreViewModel : ObservableObject
    {
        public ExploreViewModel()
        {
            LoadRoutesCommand = new AsyncRelayCommand(LoadRoutesAsync);
            ViewRouteCommand = new RelayCommand(ViewRoute);
        }

        /// <summary>
        /// The <see cref="INavigationService"/> instance currently in use.
        /// </summary>
        private readonly INavigationService NavigationService = Ioc.Default.GetRequiredService<INavigationService>();
        private readonly SettingsService SettingsService = Ioc.Default.GetRequiredService<SettingsService>();
        private readonly TamuBusFeedApi Api = Ioc.Default.GetRequiredService<TamuBusFeedApi>();

        private Route _selectedRoute;

        [System.ComponentModel.Bindable(true)]
        public ObservableCollection<Route> Routes { get; } = new ObservableCollection<Route>();
        
        [System.ComponentModel.Bindable(true)]
        public Route SelectedRoute
        {
            get => _selectedRoute;
            set => SetProperty(ref _selectedRoute, value);
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

        public async Task LoadRoutesAsync()
        {
            var targetDate = SettingsService.TargetDate;
            Routes.Clear();
            foreach (Route r in await Api.GetRoutes())
            {
                // Some of the names have leading whitespace for no reason
                r.Name = r.Name.Trim();
                await r.GetDetailedPatternAsync(targetDate);
                Routes.Add(r);
            }
        }

        public void ViewRoute()
        {
            NavigationService.Navigate("RouteView", SelectedRoute);
        }
    }
}
