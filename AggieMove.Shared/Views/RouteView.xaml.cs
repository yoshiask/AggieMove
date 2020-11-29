using AggieMove.Helpers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Linq;
using TamuBusFeed.Models;
using Windows.Foundation;
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
		public RouteView()
		{
			this.InitializeComponent();
		}

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
			ViewModel.SelectedRoute = e.Parameter as Route;

            MapHelper.LoadMap(MainMapView, MapGraphics);

            await ViewModel.LoadPatternsAsync();
            var legPoints = ViewModel.PatternElements.Select(p => new MapPoint(p.Longitude, p.Latitude, new SpatialReference(3857)));
            var legPath = new PolylineBuilder(legPoints, new SpatialReference(3857)).ToGeometry();
            // create a simple line symbol to display the polyline
            var legLineSymbol = new SimpleLineSymbol(
                SimpleLineSymbolStyle.Solid,
                System.Drawing.Color.Red,
                4.0
            );
            MapGraphics.Graphics.Add(new Graphic(legPath, legLineSymbol));

            MapHelper.SetViewpointToCurrentLocation(MainMapView, MapGraphics, Geolocator_PositionChanged);

            base.OnNavigatedTo(e);
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