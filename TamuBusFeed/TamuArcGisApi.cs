using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace TamuBusFeed
{
    public static class TamuArcGisApi
    {
        public static async Task<Route> SolveRoute(IEnumerable<MapPoint> stopPoints)
        {
            var routeTask = await RouteTask.CreateAsync(new Uri("https://gis.tamu.edu/arcgis/rest/services/Routing/20220119/NAServer/Route"));
            var routeParameters = await routeTask.CreateDefaultParametersAsync();

            var stops = stopPoints.Select(g => new Stop(g));
            routeParameters.SetStops(stops);

            // Return driving directions in user's language
            routeParameters.ReturnDirections = true;
            routeParameters.DirectionsLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            var routeResult = await routeTask.SolveRouteAsync(routeParameters);

            return routeResult?.Routes?.FirstOrDefault();
        }
    }
}
