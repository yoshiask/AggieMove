using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
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
        }

        public ObservableCollection<Route> Routes { get; } = new ObservableCollection<Route>();

        private Route selectedRoute;
        public Route SelectedRoute
        {
            get => selectedRoute;
            set => SetProperty(ref selectedRoute, value);
        }

        /// <summary>
        /// Gets the <see cref="IAsyncRelayCommand"/> instance responsible for loading posts.
        /// </summary>
        public IAsyncRelayCommand LoadRoutesCommand { get; }

        public async Task LoadRoutesAsync()
        {
            Routes.Clear();
            foreach (Route r in await TamuBusFeedApi.GetRoutes())
            {
                // Some of the names have leading whitespace for no reason
                r.Name = r.Name.Trim();
                Routes.Add(r);
            }
        }
    }
}
