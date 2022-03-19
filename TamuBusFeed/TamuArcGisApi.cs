using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Flurl;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace TamuBusFeed
{
    public static class TamuArcGisApi
    {
        public static string ApiKey { get; set; }

        public const string SERVICES_BASE = "https://gis.tamu.edu/arcgis/rest/services";
        // ILCB
        public static readonly MapPoint TamuCenter = new(-10724991.7064, 3582457.193500001, SpatialReferences.WebMercator);

        public static async Task<RouteTask> StartRouteTask()
        {
            var routeTask = await RouteTask.CreateAsync(new Uri(Url.Combine(SERVICES_BASE, "/Routing/20220119/NAServer/Route")));
            routeTask.ApiKey = ApiKey;

            return routeTask;
        }

        public static async Task<IReadOnlyList<Route>> SolveRoute(RouteTask routeTask, IEnumerable<Models.SearchResult> stopPoints,
            TravelMode travelMode = null)
        {
            var routeParameters = await routeTask.CreateDefaultParametersAsync();
            
            // Set parameters
            routeParameters.DirectionsStyle = DirectionsStyle.Campus;
            routeParameters.ReturnDirections = true;
            routeParameters.ReturnRoutes = true;
            routeParameters.ReturnStops = true;
            routeParameters.DirectionsLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            routeParameters.TravelMode = travelMode;

            var stops = stopPoints.Select(r => new Stop(r.Point) { Name = r.Name });
            routeParameters.SetStops(stops);

            var routeResult = await routeTask.SolveRouteAsync(routeParameters);

            return routeResult?.Routes;
        }

        public static async Task<IEnumerable<Models.SearchResult>> SearchAsync(string text, CancellationToken ct)
        {
            var listResults = await WhenAllSerial(ct,
                ct => SearchBuildings(text, ct), ct => SearchDepartments(text, ct),
                ct => SearchParkingGarages(text, ct), ct => SearchParkingLots(text, ct),
                ct => SearchPointsOfInterest(text, ct), ct => SearchWorld(text, ct)
            );
            return listResults.Aggregate((l1, l2) => l1.Union(l2));
        }

        public static Task<FeatureQueryResult> QueryBuildings(string text, CancellationToken ct)
        {
            text = text.ToUpperInvariant();
            string query = $"UPPER(Number) LIKE '%{text}%' OR UPPER(BldgAbbr) LIKE '%{text}%' OR UPPER(BldgName) LIKE '%{text}%'";

            var featureTable = new ServiceFeatureTable(new Uri(Url.Combine(SERVICES_BASE, "FCOR/TAMU_BaseMap/MapServer/1")));
            return Query(query, featureTable, ct);
        }

        public static Task<FeatureQueryResult> QueryBuildingsStrict(string text, CancellationToken ct)
        {
            text = text.ToUpperInvariant();
            string query = $"UPPER(BldgName) LIKE '{text}%' OR UPPER(BldgAbbr) LIKE '{text}%'";

            var featureTable = new ServiceFeatureTable(new Uri(Url.Combine(SERVICES_BASE, "FCOR/TAMU_BaseMap/MapServer/1")));
            return Query(query, featureTable, ct);
        }

        public static Task<FeatureQueryResult> QueryDepartments(string text, CancellationToken ct)
        {
            text = text.ToUpperInvariant();
            string query = $"UPPER(DeptName) LIKE '%{text}%' OR UPPER(CollegeName) LIKE '%{text}%' OR UPPER(DeptAbbre) LIKE '%{text}%'";

            var featureTable = new ServiceFeatureTable(new Uri(Url.Combine(SERVICES_BASE, "FCOR/DepartmentSearch/MapServer/1")));
            return Query(query, featureTable, ct);
        }

        public static Task<FeatureQueryResult> QueryParkingGarages(string text, CancellationToken ct)
        {
            text = text.ToUpperInvariant();
            string query = $"UPPER(LotName) LIKE '%{text}%' OR UPPER(Name) LIKE '%{text}%'";

            var featureTable = new ServiceFeatureTable(new Uri(Url.Combine(SERVICES_BASE, "FCOR/TAMU_BaseMap/MapServer/0")));
            return Query(query, featureTable, ct);
        }

        public static Task<FeatureQueryResult> QueryParkingLots(string text, CancellationToken ct)
        {
            text = text.ToUpperInvariant();
            string query = $"UPPER(LotName) LIKE '%{text}%'";

            var featureTable = new ServiceFeatureTable(new Uri(Url.Combine(SERVICES_BASE, "FCOR/TAMU_BaseMap/MapServer/12")));
            return Query(query, featureTable, ct);
        }

        public static Task<FeatureQueryResult> QueryPointsOfInterest(string text, CancellationToken ct)
        {
            text = text.ToUpperInvariant();
            string query = $"UPPER(Name) LIKE '%{text}%'";

            var featureTable = new ServiceFeatureTable(new Uri(Url.Combine(SERVICES_BASE, "FCOR/MapInfo_20190529/MapServer/0")));
            return Query(query, featureTable, ct);
        }

        public static async Task<FeatureQueryResult> Query(string query, ServiceFeatureTable featureTable, CancellationToken ct)
        {
            var queryParams = new QueryParameters()
            {
                ReturnGeometry = true,
                WhereClause = query,
            };
            var result = await featureTable.QueryFeaturesAsync(queryParams, QueryFeatureFields.LoadAll, ct);
            return result;
        }

        public static async Task<IEnumerable<Models.SearchResult>> SearchBuildings(string text, CancellationToken ct)
        {
            var results = await QueryBuildings(text, ct);
            return Models.SearchResult.FromFeatureQueryResult(
                results,
                GetAttributeValueFunction("BldgName"),
                f => string.Join(", ", f.GetAttributeValue("Address"), f.GetAttributeValue("City"), "Texas", f.GetAttributeValue("Zip")));
        }

        public static async Task<IEnumerable<Models.SearchResult>> SearchBuildingsStrict(string text, CancellationToken ct)
        {
            var results = await QueryBuildingsStrict(text, ct);
            return Models.SearchResult.FromFeatureQueryResult(results, GetAttributeValueFunction("BldgName"));
        }

        public static async Task<IEnumerable<Models.SearchResult>> SearchDepartments(string text, CancellationToken ct)
        {
            var results = await QueryDepartments(text, ct);
            string getName(Feature feature)
            {
                return feature.GetAttributeValue("CollegeName").ToString()
                    + feature.GetAttributeValue("DeptName").ToString();
            }
            return Models.SearchResult.FromFeatureQueryResult(results, getName);
        }

        public static async Task<IEnumerable<Models.SearchResult>> SearchParkingGarages(string text, CancellationToken ct)
        {
            var results = await QueryParkingGarages(text, ct);
            return Models.SearchResult.FromFeatureQueryResult(results, GetAttributeValueFunction("LotName"));
        }

        public static async Task<IEnumerable<Models.SearchResult>> SearchParkingLots(string text, CancellationToken ct)
        {
            var results = await QueryParkingLots(text, ct);
            return Models.SearchResult.FromFeatureQueryResult(results, GetAttributeValueFunction("LotName"));
        }

        public static async Task<IEnumerable<Models.SearchResult>> SearchPointsOfInterest(string text, CancellationToken ct)
        {
            var results = await QueryPointsOfInterest(text, ct);
            return Models.SearchResult.FromFeatureQueryResult(results, GetAttributeValueFunction("Name"));
        }

        public static async Task<IEnumerable<Models.SearchResult>> SearchWorld(string text, CancellationToken ct)
        {
            var locatorTask = new LocatorTask(new Uri("https://geocode-api.arcgis.com/arcgis/rest/services/World/GeocodeServer"));
            locatorTask.ApiKey = ApiKey;

            var parameters = new GeocodeParameters();
            parameters.PreferredSearchLocation = TamuCenter;
            parameters.ResultAttributeNames.Add("Score");
            parameters.ResultAttributeNames.Add("Distance");
            parameters.ResultAttributeNames.Add("Place_addr");

            var results = await locatorTask.GeocodeAsync(text, parameters, ct);
            return Models.SearchResult.FromGeocodeResults(results);
        }

        private static Func<Feature, string> GetAttributeValueFunction(string attributeName)
        {
            return feature => feature.GetAttributeValue(attributeName)?.ToString();
        }

        private static IFlurlRequest GetBase()
        {
            return (IFlurlRequest)SERVICES_BASE.SetQueryParam("f", "json");
        }

        private static async Task<TResult[]> WhenAllSerial<TResult>(params Task<TResult>[] tasks)
        {
            var results = new TResult[tasks.Length];
            for (int i = 0; i < tasks.Length; i++)
                results[i] = await tasks[i];
            return results;
        }

        private static async Task<TResult[]> WhenAllSerial<TResult>(CancellationToken ct, params Func<CancellationToken, Task<TResult>>[] tasks)
        {
            var results = new TResult[tasks.Length];
            for (int i = 0; i < tasks.Length; i++)
                results[i] = await tasks[i](ct);
            return results;
        }

        private static async IAsyncEnumerable<TResult> WhenAllSerial2<TResult>(params Task<TResult>[] tasks)
        {
            for (int i = 0; i < tasks.Length; i++)
                yield return await tasks[i];
        }

        private static async IAsyncEnumerable<TResult> WhenAllSerial2<TResult>([EnumeratorCancellation] CancellationToken ct, params Func<CancellationToken, Task<TResult>>[] tasks)
        {
            for (int i = 0; i < tasks.Length; i++)
                yield return await tasks[i](ct);
        }
    }
}
