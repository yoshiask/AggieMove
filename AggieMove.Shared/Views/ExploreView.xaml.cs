using AggieMove.Helpers;
using AggieMove.ViewModels;
using System;
using System.Linq;
using TamuBusFeed.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AggieMove.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ExploreView : Page
    {
        public ExploreView()
        {
            this.InitializeComponent();

            MainMapView.LocationDisplay.IsEnabled = true;
            MainMapView.LocationDisplay.AutoPanMode = Esri.ArcGISRuntime.UI.LocationDisplayAutoPanMode.Recenter;
            MainMapView.GeoViewTapped += MainMapView_GeoViewTapped;

            ViewModel.Routes.CollectionChanged += Routes_CollectionChanged;
        }

        private async void Routes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                MainMapView.ClearAllExceptMain();
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    Route route = item as Route;
                    if (route == null)
                        continue;
                    var rvm = new RouteViewModel(route);
                    await rvm.LoadPatternsAsync();
                    MainMapView.DrawRouteAndStops(rvm, ColorHelper.ParseCSSColorAsDrawingColor(route.Color), false);
                }
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await MainMapView.LoadMap();
        }

        private void OnRouteSelected(object sender, SelectionChangedEventArgs e)
        {
#if !NETFX_CORE
            // TODO: This is an ugly, pattern-breaking workaround because Uno fires the SelectionChanged event before
            // setting the selected item
            ViewModel.SelectedRoute = e.AddedItems[0] as TamuBusFeed.Models.Route;
#endif
        }

        private async void MainMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            await MainMapView.HandleGeoViewTapped(e);
        }
    }
}
