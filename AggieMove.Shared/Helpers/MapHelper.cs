using AggieMove.ViewModels;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace AggieMove.Helpers
{
    public static class MapHelper
    {
        public static readonly MapPoint TAMU_CENTER_POINT = new MapPoint(-96.344800, 30.610190, SpatialReferences.Wgs84);
        public static readonly string TILE_LAYER_TEMPLATE = TamuBusFeed.TamuArcGisApi.SERVICES_BASE + 
            "/FCOR/TAMU_BaseMap/MapServer/tile/{level}/{row}/{col}?blankTile=false";

        public static async Task LoadMap(MapView mapView)
        {
            var tiledLayer = new WebTiledLayer(TILE_LAYER_TEMPLATE);
            await tiledLayer.LoadAsync();
            tiledLayer.MinScale = 12000;
            tiledLayer.Attribution = "Texas A&M University Office of Mapping and Space Information";

            var basemap = Basemap.CreateImagery();
            basemap.BaseLayers.Add(tiledLayer);
            await basemap.LoadAsync();

            mapView.Map = new Map(basemap);
            await mapView.Map.LoadAsync();
            mapView.SetViewpoint(new Viewpoint(TAMU_CENTER_POINT, 36000));
        }

        public static Graphic CreateRouteStop(double lat, double lon, Color fill)
        {
            var mapPoint = new MapPoint(Convert.ToDouble(lon),
                Convert.ToDouble(lat), SpatialReferences.Wgs84);
            return CreateRouteStop(mapPoint, fill);
        }
        public static Graphic CreateRouteStop(MapPoint mapPoint, Color fill)
        {
            var pointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, fill, 20)
            {
                Outline = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.White, 5)
            };
            return new Graphic(mapPoint, pointSymbol);
        }
        public static Graphic CreateInactiveRouteStop(double lat, double lon)
        {
            return CreateRouteStop(lat, lon, Color.Black);
        }

        public static Graphic CreateRoutePath(IEnumerable<MapPoint> points, Color lineColor)
        {
            var routePath = new PolylineBuilder(points, SpatialReferences.WebMercator).ToGeometry();
            // Create a simple line symbol to display the polyline
            var routeLineSymbol = new SimpleLineSymbol(
                SimpleLineSymbolStyle.Solid, lineColor, 4.0
            );
            return new Graphic(routePath, routeLineSymbol);
        }

        public static Graphic DrawRouteAndStops(MapView mapView, RouteViewModel route, Color routeColor, bool showStops = true)
        {
            var routePoints = route.PatternElements.Select(p => new MapPoint(p.Longitude, p.Latitude, SpatialReferences.WebMercator));
            Graphic routePath = CreateRoutePath(routePoints, routeColor);
            var routeOverlay = new GraphicsOverlay
            {
                Id = "route_" + route.SelectedRoute.ShortName
            };
            routeOverlay.Graphics.Add(routePath);

            if (showStops)
                foreach (TamuBusFeed.Models.PatternElement elem in route.Stops)
                {
                    var point = new MapPoint(elem.Longitude, elem.Latitude, SpatialReferences.WebMercator);
                    var stop = CreateRouteStop(point, routeColor);
                    routeOverlay.Graphics.Add(stop);
                }

            mapView.GraphicsOverlays.Add(routeOverlay);
            return routePath;
        }

        public static Graphic DrawDirections(MapView mapView, Route route, Color routeColor, bool showStops = true)
        {
            var routeLineSymbol = new SimpleLineSymbol(
                SimpleLineSymbolStyle.Solid, routeColor, 4.0
            );
            Graphic routePath = new Graphic(route.RouteGeometry, routeLineSymbol);
            var routeOverlay = new GraphicsOverlay
            {
                Id = "directions_" + route.GetHashCode().ToString()
            };
            routeOverlay.Graphics.Add(routePath);

            if (showStops)
                foreach (var elem in route.Stops)
                {
                    var point = elem.Geometry;
                    var stop = CreateRouteStop(point, routeColor);
                    routeOverlay.Graphics.Add(stop);
                }

            mapView.GraphicsOverlays.Add(routeOverlay);
            return routePath;
        }

        public static void ClearAllRouteOverlays(MapView mapView)
        {
            int i = 0;
            while (i < mapView.GraphicsOverlays.Count)
            {
                var overlay = mapView.GraphicsOverlays[i];
                if (overlay.Id != null && overlay.Id.StartsWith("route_"))
                    mapView.GraphicsOverlays.RemoveAt(i);
                else
                    i++;
            }
        }

        public static void ClearAllExceptMain(MapView mapView)
        {
            for (int i = 0; i < mapView.GraphicsOverlays.Count; i++)
            {
                var overlay = mapView.GraphicsOverlays[i];
                if (overlay.Id == null || overlay.Id == "MapGraphics")
                    continue;

                mapView.GraphicsOverlays.RemoveAt(i--);
            }
        }
    }
}
