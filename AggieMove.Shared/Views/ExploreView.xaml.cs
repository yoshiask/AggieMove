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

            ViewModel.Routes.CollectionChanged += Routes_CollectionChanged;
        }

        private async void Routes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                MapHelper.ClearAllExceptMain(MainMapView);
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
                    MapHelper.DrawRouteAndStops(MainMapView, rvm, ColorHelper.ParseCSSColorAsDrawingColor(route.Color), false);
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MapHelper.LoadMap(MainMapView);
        }

        private void OnRouteSelected(object sender, SelectionChangedEventArgs e)
        {
#if !NETFX_CORE
            // TODO: This is an ugly, pattern-breaking workaround because Uno fires the SelectionChanged event before
            // setting the selected item
            ViewModel.SelectedRoute = e.AddedItems[0] as TamuBusFeed.Models.Route;
#endif
        }
    }
}
