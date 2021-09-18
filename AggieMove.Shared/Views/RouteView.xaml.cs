using AggieMove.Helpers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Linq;
using TamuBusFeed.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AggieMove.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RouteView : Page
    {
        public System.Drawing.Color DrawingColor { get; set; }

        public RouteView()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.SelectedRoute = e.Parameter as Route;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            MapHelper.LoadMap(MainMapView);

            await ViewModel.LoadPatternsAsync();
            DrawingColor = ColorHelper.ParseCSSColorAsDrawingColor(ViewModel.SelectedRoute.Color);

            bool hasRoutePoints = ViewModel.PatternElements.Count > 0;
            if (hasRoutePoints)
            {
                MapHelper.DrawRouteAndStops(MainMapView, ViewModel, DrawingColor).ContinueWith(async (Task) =>
                {
                    MainMapView.SetViewpointGeometryAsync(Task.Result.Geometry);
                });
            }
            else
            {
                MapHelper.SetViewpointToCurrentLocation(MainMapView, MapGraphics, Geolocator_PositionChanged, !hasRoutePoints);
            }

            // Show time table
            await ViewModel.LoadTimeTableAsync();
            Windows.UI.Xaml.Controls.Grid ttGrid;
            if (ViewModel.TimeTable != null)
            {
                ttGrid = TimeTableUIFactory.CreateGridFromTimeTable(ViewModel.TimeTable);
            }
            else
            {
                ttGrid = new Windows.UI.Xaml.Controls.Grid
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "Unable to load stop timing."
                        }
                    }
                };
            }
            TimeTablePresenter.Content = ttGrid;

            // Bring current time into view
            // TODO: Extend this to include the closest time
            string nowTime = DateTime.Now.ToString("hh:mm tt");
            TextBlock nowBlock = ttGrid.Children.FirstOrDefault(ui => (string)(ui as FrameworkElement)?.Tag == nowTime) as TextBlock;
            if (ViewModel.TimeTable != null && nowBlock != null)
            {
                nowBlock.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 200, 0, 0));
                nowBlock.StartBringIntoView();
            }

            base.OnNavigatedTo(e);
        }

        private async void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.SelectedPatternElement) && ViewModel.SelectedPatternElement != null)
            {
                PatternElement elem = ViewModel.SelectedPatternElement;
                var point = new MapPoint(elem.Longitude, elem.Latitude, MapHelper.BUS_ROUTES_SR);
                await MainMapView.SetViewpointCenterAsync(point);
                await MainMapView.SetViewpointScaleAsync(2000);
            }
        }

        private async void Geolocator_PositionChanged(Windows.Devices.Geolocation.Geolocator sender, Windows.Devices.Geolocation.PositionChangedEventArgs args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                MapGraphics.Graphics.Where(g =>
                {
                    if (g.Attributes.ContainsKey("id"))
                    {
                        return (string)g.Attributes["id"] != "currentLocation";
                    }
                    return true;
                });

                var currentLocation = MapHelper.CreateRouteStop(
                    args.Position.Coordinate.Point.Position.Latitude,
                    args.Position.Coordinate.Point.Position.Longitude,
                    System.Drawing.Color.Red
                );
                currentLocation.Attributes.Add("id", "currentLocation");
                MapGraphics.Graphics.Add(currentLocation);
            });
        }
    }
}