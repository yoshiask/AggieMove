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
        }

        public ObservableCollection<PatternElement> Stops { get; } = new ObservableCollection<PatternElement>();

        private Route _SelectedRoute;
        public Route SelectedRoute
        {
            get => _SelectedRoute;
            set => SetProperty(ref _SelectedRoute, value);
        }

        /// <summary>
        /// Gets the <see cref="IAsyncRelayCommand"/> instance responsible for loading patterns for the selected route.
        /// </summary>
        public IAsyncRelayCommand LoadPatternsCommand { get; }

        public async Task LoadPatternsAsync()
        {
            Stops.Clear();
            foreach (PatternElement p in await TamuBusFeedApi.GetPattern(SelectedRoute.ShortName))
            {
                // Some of the names have leading whitespace for no reason
                if (p.RouteHeaderRank == -1)
                    continue;
                p.Name = p.Name.Trim();
                Stops.Add(p);
            }
        }
    }
}
