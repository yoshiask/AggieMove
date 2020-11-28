using Microsoft.Toolkit.Mvvm.ComponentModel;
using TamuBusFeed.Models;

namespace AggieMove.ViewModels
{
    public class RouteViewModel : ObservableRecipient
    {
        private Route _SelectedRoute;
        public Route SelectedRoute
        {
            get => _SelectedRoute;
            set => SetProperty(ref _SelectedRoute, value);
        }
    }
}
