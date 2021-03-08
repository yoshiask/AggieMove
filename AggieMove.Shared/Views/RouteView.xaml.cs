using AggieMove.Helpers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Linq;
using TamuBusFeed.Models;
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

            bool hasRoutePoints = ViewModel.PatternElements.Count != 0;
            if (hasRoutePoints)
            {
                var routePoints = ViewModel.PatternElements.Select(p => new MapPoint(p.Longitude, p.Latitude, MapHelper.BUS_ROUTES_SR));
                var routePath = new PolylineBuilder(routePoints, MapHelper.BUS_ROUTES_SR).ToGeometry();
                // Create a simple line symbol to display the polyline
                var routeLineSymbol = new SimpleLineSymbol(
                    SimpleLineSymbolStyle.Solid, DrawingColor, 4.0
                );
                MapGraphics.Graphics.Add(new Graphic(routePath, routeLineSymbol));
                await MainMapView.SetViewpointGeometryAsync(routePath);

                foreach (PatternElement elem in ViewModel.Stops)
                {
                    var point = new MapPoint(elem.Longitude, elem.Latitude, MapHelper.BUS_ROUTES_SR);
                    var stop = MapHelper.CreateRouteStop(point, DrawingColor);
                    MapGraphics.Graphics.Add(stop);
                }
            }

            MapHelper.SetViewpointToCurrentLocation(MainMapView, MapGraphics, Geolocator_PositionChanged, !hasRoutePoints);

            // Show time table
            await ViewModel.LoadTimeTableAsync();
            TimeTablePresenter.Content = TimeTableUIFactory.CreateGridFromTimeTable(ViewModel.TimeTable);

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