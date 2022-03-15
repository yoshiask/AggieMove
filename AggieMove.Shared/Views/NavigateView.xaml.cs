using AggieMove.Helpers;
using AggieMove.ViewModels;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using tkColorHelper = Microsoft.Toolkit.Uwp.Helpers.ColorHelper;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AggieMove.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NavigateView : Page
    {
        public NavigateView()
        {
            ViewModel = new NavigateViewModel();
            ViewModel.Stops.CollectionChanged += Stops_CollectionChanged;
            ViewModel.Routes.CollectionChanged += Routes_CollectionChanged;

            Loaded += Page_Loaded;
            this.InitializeComponent();

            MainMapView.LocationDisplay.IsEnabled = true;
            MainMapView.LocationDisplay.AutoPanMode = Esri.ArcGISRuntime.UI.LocationDisplayAutoPanMode.Recenter;
        }

        public NavigateViewModel ViewModel
        {
            get => (NavigateViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel), typeof(NavigateViewModel), typeof(NavigateView), new PropertyMetadata(null));

        private void Stops_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                MapGraphics.Graphics.Clear();
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    var stop = (TamuBusFeed.Models.SearchResult)item;
                    var stopGraphic = MapHelper.CreateRouteStop(stop.Point, System.Drawing.Color.Blue);
                    MapGraphics.Graphics.Add(stopGraphic);
                }
            }
        }

        private void Routes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                MapGraphics.Graphics.Clear();
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                if (!(sender is System.Collections.ICollection routes))
                    return;

                for (int i = 0; i < e.NewItems.Count; i++)
                {
                    var item = e.NewItems[i];
                    if (!(item is Route route))
                        continue;

                    double hue = 360 * i / routes.Count;
                    var color = tkColorHelper.FromHsl(hue, 0.75, 0.60).ToDrawingColor();
                    MainMapView.DrawDirections(route, color, true);
                }
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await MainMapView.LoadMap();
        }
    }
}
