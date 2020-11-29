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

        public ObservableCollection<PatternElement> PatternElements { get; } = new ObservableCollection<PatternElement>();

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
            PatternElements.Clear();
            foreach (PatternElement p in await TamuBusFeedApi.GetPattern(SelectedRoute.ShortName))
            {
                p.Name = p.Name.Trim();
                PatternElements.Add(p);

                // Some of the names have leading whitespace for no reason
                if (p.Stop != null)
                    Stops.Add(p);
            }
        }
    }
}
