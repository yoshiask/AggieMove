using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Flurl;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace TamuBusFeed
{
    public static class TamuArcGisApi
    {
        public const string SERVICES_BASE = "https://gis.tamu.edu/arcgis/rest/services";

        public static async Task<IReadOnlyList<Route>> SolveRoute(IEnumerable<MapPoint> stopPoints)
        {
            var routeTask = await RouteTask.CreateAsync(new Uri(Url.Combine(SERVICES_BASE, "/Routing/20220119/NAServer/Route")));
            var routeParameters = await routeTask.CreateDefaultParametersAsync();

            var stops = stopPoints.Select(g => new Stop(g));
            routeParameters.SetStops(stops);

            // Return driving directions in user's language
            routeParameters.ReturnDirections = true;
            routeParameters.DirectionsLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            var routeResult = await routeTask.SolveRouteAsync(routeParameters);

            return routeResult?.Routes;
        }

        public static Task<FeatureQueryResult> QueryBuildings(string text)
        {
            text = text.ToUpperInvariant();
            string query = $"UPPER(Number) LIKE '%{text}%' OR UPPER(BldgAbbr) LIKE '%{text}%' OR UPPER(BldgName) LIKE '%{text}%'";

            var featureTable = new ServiceFeatureTable(new Uri(Url.Combine(SERVICES_BASE, "FCOR/TAMU_BaseMap/MapServer/1")));
            return Query(query, featureTable);
        }

        public static Task<FeatureQueryResult> QueryBuildingsStrict(string text)
        {
            text = text.ToUpperInvariant();
            string query = $"UPPER(BldgName) LIKE '{text}%' OR UPPER(BldgAbbr) LIKE '{text}%'";

            var featureTable = new ServiceFeatureTable(new Uri(Url.Combine(SERVICES_BASE, "FCOR/TAMU_BaseMap/MapServer/1")));
            return Query(query, featureTable);
        }

        public static Task<FeatureQueryResult> QueryDepartments(string text)
        {
            text = text.ToUpperInvariant();
            string query = $"UPPER(DeptName) LIKE '%{text}%' OR UPPER(CollegeName) LIKE '%{text}%' OR UPPER(DeptAbbre) LIKE '%{text}%'";

            var featureTable = new ServiceFeatureTable(new Uri(Url.Combine(SERVICES_BASE, "FCOR/DepartmentSearch/MapServer/1")));
            return Query(query, featureTable);
        }

        public static Task<FeatureQueryResult> QueryParkingGarages(string text)
        {
            text = text.ToUpperInvariant();
            string query = $"UPPER(LotName) LIKE '%{text}%' OR UPPER(Name) LIKE '%{text}%'";

            var featureTable = new ServiceFeatureTable(new Uri(Url.Combine(SERVICES_BASE, "FCOR/TAMU_MapServer/MapServer/0")));
            return Query(query, featureTable);
        }

        public static Task<FeatureQueryResult> QueryParkingLots(string text)
        {
            text = text.ToUpperInvariant();
            string query = $"UPPER(LotName) LIKE '%{text}%'";

            var featureTable = new ServiceFeatureTable(new Uri(Url.Combine(SERVICES_BASE, "FCOR/TAMU_MapServer/MapServer/12")));
            return Query(query, featureTable);
        }

        public static Task<FeatureQueryResult> QueryPointsOfInterest(string text)
        {
            text = text.ToUpperInvariant();
            string query = $"UPPER(Name) LIKE '%{text}%'";

            var featureTable = new ServiceFeatureTable(new Uri(Url.Combine(SERVICES_BASE, "FCOR/MapInfo_20190529/MapServer/0")));
            return Query(query, featureTable);
        }

        public static async Task<FeatureQueryResult> Query(string query, ServiceFeatureTable featureTable)
        {
            var queryParams = new QueryParameters()
            {
                ReturnGeometry = true,
                WhereClause = query,
            };
            return await featureTable.QueryFeaturesAsync(queryParams, QueryFeatureFields.LoadAll);
        }

        private static IFlurlRequest GetBase()
        {
            return (IFlurlRequest)SERVICES_BASE.SetQueryParam("f", "json");
        }
    }
}
