using AggieMove.ViewModels;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Mapping.Popups;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TamuBusFeed.Models;
using Uno.Extensions;

namespace AggieMove.Helpers
{
    public static class MapHelper
    {
        public static readonly string TILE_LAYER_TEMPLATE = TamuBusFeed.TamuArcGisApi.BaspMapUrl.TrimEnd('/') + 
            "/tile/{level}/{row}/{col}";

        public const string VehiclesLayerId = "vehicles";

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
            mapView.SetViewpoint(new Viewpoint(TamuBusFeed.TamuArcGisApi.TamuCenter, 36000));
        }

        public static async Task HandleGeoViewTapped(this MapView mapView, GeoViewInputEventArgs e)
        {
            // Identify graphics using the screen tap.
            var screenPoint = e.Position;
            var resultGraphics = await mapView.IdentifyGraphicsOverlaysAsync(screenPoint, 10, false);

            // Show details in a callout for the first graphic identified (if any).
            var result = resultGraphics.FirstOrDefault();
            if (result != null && result.Graphics.Count > 0)
            {
                var poiPopup = result.Popups.FirstOrDefault();

                if (poiPopup != null)
                {
                    result.GraphicsOverlay.PopupDefinition = poiPopup.PopupDefinition;
                    result.GraphicsOverlay.IsPopupEnabled = true;
                }

                var poiGraphic = result.Graphics.First();
                var callout = CreateCallout(poiGraphic);

                var popup = new Popup(poiGraphic, poiGraphic.Attributes["Popup"] as PopupDefinition);
                Esri.ArcGISRuntime.

                MapPoint calloutAnchor = poiGraphic.Geometry.GetClosestPoint(e.Location);
                mapView.ShowCalloutAt(calloutAnchor, callout);
            }
            else
            {
                mapView.DismissCallout();
            }
        }

        public static GraphicsOverlay CreateVehiclesOverlay(this MapView mapView)
        {
            var vehiclesLayer = new GraphicsOverlay
            {
                Id = VehiclesLayerId,
            };

            mapView.GraphicsOverlays.Add(vehiclesLayer);
            return vehiclesLayer;
        }

        public static Graphic CreateRouteStop(double lat, double lon, Color fill)
        {
            var mapPoint = new MapPoint(lon, lat, SpatialReferences.Wgs84);
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
            var mapPoint = new MapPoint(lon, lat);
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
                Id = "route_" + route.SelectedRoute.ShortName
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
            vehicleGraphic.Attributes.Add("Description", GetVehicleCalloutDescription(vehicle));

            vehicle.PropertyChanged += Vehicle_PropertyChanged;
            vehicle.Graphic = vehicleGraphic;

            var vehiclesOverlay = mapView.GraphicsOverlays[VehiclesLayerId];
            vehiclesOverlay.Graphics.Add(vehicleGraphic);

            return vehicleGraphic;
        }

        public static void RemoveVehicle(this MapView mapView, string key)
        {
            var vehiclesOverlay = mapView.GraphicsOverlays[VehicleOverlayIdPrefix];

            // ArcGIS UWP can take a predicate directly, Android cannot
            vehiclesOverlay.Graphics.Remove(g => g.Attributes["VehicleId"]?.ToString() == key);
        }

        public static void ClearAllRouteOverlays(this MapView mapView)
            => mapView.GraphicsOverlays.RemoveAll(overlay => overlay.Id != null && overlay.Id.StartsWith("route_"));

        public static void ClearVehiclesOverlay(this MapView mapView)
        {
            var vehiclesOverlay = mapView.GraphicsOverlays[VehiclesLayerId];
            vehiclesOverlay.Graphics.Clear();
        }

        public static void ClearAllExceptMain(this MapView mapView)
            => mapView.GraphicsOverlays.RemoveAll(overlay => overlay.Id == null || overlay.Id == "MapGraphics");

        public static CalloutDefinition CreateCallout(Esri.ArcGISRuntime.Data.GeoElement elem)
        {
            CalloutDefinition callout = new CalloutDefinition(elem)
            {
                Text = elem.Attributes["Title"]?.ToString(),
                DetailText = elem.Attributes["Description"]?.ToString(),
            };

            return callout;
        }

        public static string GetVehicleCalloutDescription(VehicleViewModel vehicle)
        {
            var apc = vehicle.Mentor.APC;
            var nextStop = vehicle.Mentor.NextStops.FirstOrDefault();

            StringBuilder desc = new($"{apc.TotalPassenger} of {apc.PassengerCapacity} passengers");

            if (nextStop != null)
                desc.AppendLine($"Next stop: {nextStop.Name}");

            if (vehicle.Speed > 0)
                desc.AppendLine($"Speed: {vehicle.Speed}");

            return desc.ToString();
        }

        public static PopupDefinition CreateVehiclePopup(VehicleViewModel vehicle)
        {
            var route = vehicle.Mentor.CurrentWork.Route;
            var apc = vehicle.Mentor.APC;
            var nextStop = vehicle.Mentor.NextStops.FirstOrDefault();

            PopupDefinition popup = new PopupDefinition()
            {
                Title = "Bus",
            };
            popup.Elements.Add(new TextPopupElement($"**{route.RouteNumber} {route.Name}**"));
            popup.Elements.Add(new TextPopupElement($"{apc.TotalPassenger} of {apc.PassengerCapacity} passengers"));

            if (nextStop != null)
                popup.Elements.Add(new TextPopupElement($"Next stop: {nextStop.Name}"));

            if (vehicle.Speed > 0)
                popup.Elements.Add(new TextPopupElement($"Speed: {vehicle.Speed}"));

            return popup;
        }

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

        public static MapPoint GetClosestPoint(this Geometry geo, MapPoint target)
        {
            if (geo is MapPoint pt)
            {
                return pt;
            }
            else if (geo is Polyline pl)
            {
                return pl.Parts.SelectMany(part => part.Points)
                    .MinElement(pt => pt.Distance(target));
            }
            else if (geo is Polygon pg)
            {
                return pg.Extent.IsWithin(target) ? target : pg.Extent.GetCenter();
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

        private static void Vehicle_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(VehicleViewModel.Speed) || sender is not VehicleViewModel vehicle)
                return;

            vehicle.Graphic.Attributes["Description"] = GetVehicleCalloutDescription(vehicle);

            var gps = vehicle.GpsHistory.First.Value;
            vehicle.Graphic.Geometry = new MapPoint(gps.Long, gps.Lat);
            if (vehicle.Graphic.Symbol is MarkerSymbol markerSymbol)
                markerSymbol.Angle = gps.Dir;
        }
    }
}
