﻿using AggieMove.ViewModels;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using TamuBusFeed;
using TamuBusFeed.Models;
using Uno.Extensions;

namespace AggieMove.Helpers
{
    public static partial class MapHelper
    {
        public static readonly string TILE_LAYER_TEMPLATE = TamuBusFeed.TamuArcGisApi.BaseMapUrl.TrimEnd('/') +
            "/tile/{level}/{row}/{col}";

        public const string RouteOverlayIdPrefix = "route_";
        public const string VehicleOverlayIdPrefix = "vehicles_";

        public static async Task LoadMap(this MapView mapView)
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
            mapView.SetViewpoint(new Viewpoint(TamuArcGisApi.TamuCenter, 36000));
        }

        public static async Task HandleGeoViewTapped(this MapView mapView, GeoViewInputEventArgs e)
        {
            // Clear any previously selected items.
            mapView.GraphicsOverlays.ForEach(go => go.ClearSelection());
            mapView.DismissCallout();

            // Identify graphics using the screen tap.
            var resultGraphics = await mapView.IdentifyGraphicsOverlaysAsync(e.Position, 10, false);

            // Show details in a callout for the first graphic identified (if any).
            var result = resultGraphics.FirstOrDefault();
            if (result != null && result.Graphics.Count > 0)
            {
                result.GraphicsOverlay.SelectGraphics(result.Graphics);

                var poiGraphic = result.Graphics.First();
                var callout = CreateCallout(poiGraphic, mapView);

                MapPoint calloutAnchor = GeometryEngine.NearestCoordinate(poiGraphic.Geometry, e.Location).Coordinate;
                mapView.ShowCalloutAt(calloutAnchor, callout);
            }
        }

        public static Graphic CreateRouteStop(double lat, double lon, Color fill)
        {
            var mapPoint = new MapPoint(lon, lat, TamuArcGisApi.TamuSpatialReference);
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

        public static Graphic CreateVehicle(double lat, double lon, double heading)
        {
            var pointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Triangle, Color.Maroon, 20)
            {
                Outline = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.White, 5),
                Angle = heading,
                AngleAlignment = SymbolAngleAlignment.Map,
            };
            var mapPoint = new MapPoint(lon, lat, TamuArcGisApi.TamuSpatialReference);
            return new Graphic(mapPoint, pointSymbol);
        }

        public static Graphic DrawRouteAndStops(this MapView mapView, RouteViewModel route, Color routeColor, bool showStops = true)
        {
            var routePoints = route.PatternElements.Select(p => new MapPoint(p.Longitude, p.Latitude, SpatialReferences.WebMercator));
            Graphic routePath = CreateRoutePath(routePoints, routeColor);
            routePath.Attributes.Add("Title", route.SelectedRoute.ShortName);
            routePath.Attributes.Add("Description", route.SelectedRoute.Name);

            var routeOverlay = new GraphicsOverlay
            {
                Id = RouteOverlayIdPrefix + route.SelectedRoute.ShortName
            };
            routeOverlay.Graphics.Add(routePath);

            if (showStops)
                foreach (PatternElement elem in route.Stops)
                {
                    var point = new MapPoint(elem.Longitude, elem.Latitude, SpatialReferences.WebMercator);
                    var stop = CreateRouteStop(point, routeColor);

                    string title = elem.Name;
                    if (elem.Stop.IsTimePoint)
                        title = "⏱ " + title;

                    stop.Attributes.Add("Title", title);
                    stop.Attributes.Add("Description", elem.Stop.StopCode);

                    routeOverlay.Graphics.Add(stop);
                }

            mapView.GraphicsOverlays.Add(routeOverlay);
            return routePath;
        }

        public static Graphic DrawDirections(this MapView mapView, Esri.ArcGISRuntime.Tasks.NetworkAnalysis.Route route, Color routeColor, bool showStops = true)
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

        public static Graphic DrawVehicle(this MapView mapView, VehicleViewModel vehicle)
        {
            var route = vehicle.Mentor.CurrentWork.Route;

            Graphic vehicleGraphic = CreateVehicle(vehicle.Mentor.GPS.Lat, vehicle.Mentor.GPS.Long, vehicle.Mentor.GPS.Dir);
            vehicleGraphic.Attributes.Add("VehicleId", vehicle.Mentor.Key);
            vehicleGraphic.Attributes.Add("Title", $"{route.RouteNumber} {route.Name}");
            vehicleGraphic.Attributes.Add("Description", vehicle.Description);

            vehicle.MentorUpdated += Vehicle_MentorUpdated;
            vehicle.Graphic = vehicleGraphic;

            var vehicleOverlay = new GraphicsOverlay
            {
                Id = VehicleOverlayIdPrefix + vehicle.Mentor.Key
            };
            vehicleOverlay.Graphics.Add(vehicleGraphic);

            mapView.GraphicsOverlays.Add(vehicleOverlay);

            return vehicleGraphic;
        }

        public static void RemoveVehicle(this MapView mapView, string key)
        {
            var vehiclesOverlay = mapView.GraphicsOverlays[VehicleOverlayIdPrefix];
            vehiclesOverlay.Graphics.Remove(g => g.Attributes["VehicleId"]?.ToString() == key);
        }

        public static void ClearAllRouteOverlays(this MapView mapView)
            => mapView.GraphicsOverlays.RemoveAll(overlay => overlay.Id != null && overlay.Id.StartsWith(RouteOverlayIdPrefix));

        public static void ClearAllVehicleOverlays(this MapView mapView)
            => mapView.GraphicsOverlays.RemoveAll(overlay => overlay.Id != null && overlay.Id.StartsWith(VehicleOverlayIdPrefix));

        public static void ClearAllExceptMain(this MapView mapView)
            => mapView.GraphicsOverlays.RemoveAll(overlay => overlay.Id == null || overlay.Id == "MapGraphics");

        public static MapPoint GetMedianPoint(this Geometry geo)
        {
            if (geo is MapPoint pt)
            {
                return pt;
            }
            else if (geo is Polyline pl)
            {
                var medianPart = pl.Parts[pl.Parts.Count / 2];
                var medianSegment = medianPart[medianPart.SegmentCount / 2];
                if (medianSegment is LineSegment line)
                {
                    return AveragePoint(line.StartPoint, line.EndPoint);
                }
                else if (medianSegment is CubicBezierSegment cubic)
                {
                    MapPoint B(double t)
                    {
                        double t2 = t * t;
                        double t3 = t2 * t;
                        double invT = 1 - t;
                        double invT2 = invT * invT;
                        double invT3 = invT2 * invT;

                        var term1 = cubic.StartPoint.Multiply(invT3);
                        var term2 = cubic.ControlPoint1.Multiply(invT2 * t);
                        var term3 = cubic.ControlPoint2.Multiply(invT * t2);
                        var term4 = cubic.EndPoint.Multiply(t3);

                        return term1.Add(term2).Add(term3).Add(term4);
                    }
                    return B(0.5);
                }
                //else if (medianSegment is EllipticArcSegment arc)
                //{

                //}
            }
            else if (geo is Polygon pg)
            {
                return pg.Extent.GetCenter();
            }

            throw new NotImplementedException();
        }

        public static MapPoint AveragePoint(params MapPoint[] points)
        {
            int numPts = points.Length;
            double x = points.Sum(pt => pt.X) / numPts;
            double y = points.Sum(pt => pt.Y) / numPts;
            double z = points.Sum(pt => pt.Z) / numPts;
            return new MapPoint(x, y, x, points[0].SpatialReference);
        }

        public static MapPoint Add(this MapPoint pt1, MapPoint pt2)
        {
            return new MapPoint(pt1.X + pt2.X, pt1.Y + pt2.Y, pt1.Z + pt2.Z, pt1.SpatialReference);
        }

        public static MapPoint Multiply(this MapPoint pt1, double scalar)
        {
            return new MapPoint(pt1.X * scalar, pt1.Y * scalar, pt1.Z * scalar, pt1.SpatialReference);
        }

        public static double Distance(this MapPoint pt1, MapPoint pt2)
        {
            double dX = pt1.X - pt2.X;
            double dY = pt1.Y - pt2.Y;
            double dZ = pt1.Z - pt2.Z;
            return Math.Sqrt((dX * dX) + (dY * dY) + (dZ * dZ));
        }

        public static bool IsWithin(this Envelope env, MapPoint pt)
        {
            return (pt.X >= env.XMin) && (pt.X <= env.XMax)
                && (pt.Y >= env.YMin) && (pt.Y <= env.YMax);
        }

        public static T MinElement<T>(this IEnumerable<T> list, Func<T, double> selector)
        {
            T minElem = default;
            double minVal = double.MaxValue;

            foreach (T curElem in list)
            {
                double curVal = selector(curElem);
                if (curVal < minVal)
                {
                    minElem = curElem;
                    minVal = curVal;
                }
            }

            return minElem;
        }

        private static void Vehicle_MentorUpdated(VehicleViewModel vehicle)
        {
            vehicle.Graphic.Attributes["Description"] = vehicle.Description;

            var gps = vehicle.GpsHistory.First.Value;
            var graphic = vehicle.Graphic;
            graphic.Geometry = new MapPoint(gps.Long, gps.Lat, TamuArcGisApi.TamuSpatialReference);
            if (graphic.Symbol is MarkerSymbol markerSymbol)
                markerSymbol.Angle = gps.Dir;
        }
    }
}
