using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AggieMove.Helpers
{
    public static class MapHelper
    {
        public static readonly MapPoint TAMU_CenterPoint = new MapPoint(30.610190 , - 96.344800);

        public static async Task<RouteResult> LoadRouter(MapView mapView)
        {
            var routeSourceUri = new Uri("https://gis.tamu.edu/arcgis/rest/services/Routing/20201128/NAServer/Route");
            var routeTask = await RouteTask.CreateAsync(routeSourceUri);

            // get the default route parameters
            var routeParams = await routeTask.CreateDefaultParametersAsync();
            // explicitly set values for some params
            routeParams.ReturnDirections = true;
            routeParams.ReturnRoutes = true;
            if (mapView.SpatialReference != null)
                routeParams.OutputSpatialReference = mapView.SpatialReference;

            // create a Stop for my location
            var curLoc = await SpatialHelper.GetCurrentLocation();
            var myLocation = new MapPoint(curLoc.Value.Longitude, curLoc.Value.Latitude, SpatialReferences.Wgs84);
            var stop1 = new Stop(myLocation);

            // create a Stop for your location
            var yourLocation = new MapPoint(-96.34131, 30.61247, SpatialReferences.Wgs84);
            var stop2 = new Stop(yourLocation);

            // assign the stops to the route parameters
            var stopPoints = new List<Stop> { stop1, stop2 };
            routeParams.SetStops(stopPoints);

            try
            {
                return await routeTask.SolveRouteAsync(routeParams);
            }
            catch
            {
                return null;
            }
        }

        public static void LoadMap(double lat, double lon, MapView mapView, GraphicsOverlay mapGraphics)
        {
            mapView.Map = new Map(
                BasemapType.ImageryWithLabels,
                lat, lon,
                12
            );

            // Display all buildings
            var buildingsAUri = new Uri("https://gis.tamu.edu/arcgis/rest/services/FCOR/TAMU_BaseMap/MapServer/2");
            var buildingsALayer = new FeatureLayer(new ServiceFeatureTable(buildingsAUri));
            mapView.Map.OperationalLayers.Add(buildingsALayer);
            var buildingsBUri = new Uri("https://gis.tamu.edu/arcgis/rest/services/FCOR/TAMU_BaseMap/MapServer/3");
            var buildingsBLayer = new FeatureLayer(new ServiceFeatureTable(buildingsBUri));
            mapView.Map.OperationalLayers.Add(buildingsBLayer);
        }
        public static void LoadMap(MapView mapView, GraphicsOverlay mapGraphics)
        {
            LoadMap(TAMU_CenterPoint.X, TAMU_CenterPoint.Y, mapView, mapGraphics);
        }

        public static async Task SetViewpointToCurrentLocation(MapView mapView, GraphicsOverlay mapGraphics, Windows.Foundation.TypedEventHandler<Windows.Devices.Geolocation.Geolocator, Windows.Devices.Geolocation.PositionChangedEventArgs> PositionChangedHandler)
        {
            var currentLoc = await SpatialHelper.GetCurrentLocation();
            if (currentLoc.HasValue)
            {
                var currentLocPoint = CreateRouteStop(currentLoc.Value.Latitude, currentLoc.Value.Longitude, System.Drawing.Color.Red);
                mapGraphics.Graphics.Add(currentLocPoint);
                await mapView.SetViewpointCenterAsync(currentLoc.Value.Latitude, currentLoc.Value.Longitude);
                await mapView.SetViewpointScaleAsync(2000);

                if (PositionChangedHandler != null)
                    SpatialHelper.Geolocator.PositionChanged += PositionChangedHandler;
            }
        }

        public static Graphic CreateRouteStop(double lat, double lon, System.Drawing.Color fill)
        {
            // Now draw a point where the stop is
            var mapPoint = new MapPoint(Convert.ToDouble(lon),
                Convert.ToDouble(lat), SpatialReferences.Wgs84);
            var pointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, fill, 20);
            pointSymbol.Outline = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.White, 5);
            return new Graphic(mapPoint, pointSymbol);
        }
        public static Graphic CreateInactiveRouteStop(double lat, double lon)
        {
            return CreateRouteStop(lat, lon, System.Drawing.Color.Black);
        }
    }
}
