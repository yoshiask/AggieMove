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
        public const string SERVICES_BASE = "https://gis.tamu.edu/arcgis/rest/services";
        // ILCB
        public static readonly MapPoint TamuCenter = new(-10724991.7064, 3582457.193500001, SpatialReference.Create(3857));

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

        public static async Task<IEnumerable<Models.SearchResult>> SearchAsync(string text, [EnumeratorCancellation] CancellationToken ct)
        {
            var listResults = await WhenAllSerial(ct,
                ct => SearchBuildings(text, ct), ct => SearchDepartments(text, ct),
                ct => SearchParkingGarages(text, ct), ct => SearchParkingLots(text, ct),
                ct => SearchPointsOfInterest(text, ct), ct => SearchWorld(text, ct)
            );
            return listResults.Aggregate((l1, l2) => l1.Union(l2));
            //foreach (var results in listResults)
            //    foreach (var result in results)
            //        yield return result;
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
            return Models.SearchResult.FromFeatureQueryResult(results, GetNameFunctionFromAttribute("BldgName"));
        }

        public static async Task<IEnumerable<Models.SearchResult>> SearchBuildingsStrict(string text, CancellationToken ct)
        {
            var results = await QueryBuildingsStrict(text, ct);
            return Models.SearchResult.FromFeatureQueryResult(results, GetNameFunctionFromAttribute("BldgName"));
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
            return Models.SearchResult.FromFeatureQueryResult(results, GetNameFunctionFromAttribute("LotName"));
        }

        public static async Task<IEnumerable<Models.SearchResult>> SearchParkingLots(string text, CancellationToken ct)
        {
            var results = await QueryParkingLots(text, ct);
            return Models.SearchResult.FromFeatureQueryResult(results, GetNameFunctionFromAttribute("LotName"));
        }

        public static async Task<IEnumerable<Models.SearchResult>> SearchPointsOfInterest(string text, CancellationToken ct)
        {
            var results = await QueryPointsOfInterest(text, ct);
            return Models.SearchResult.FromFeatureQueryResult(results, GetNameFunctionFromAttribute("Name"));
        }

        public static async Task<IEnumerable<Models.SearchResult>> SearchWorld(string text, CancellationToken ct)
        {
            var locatorTask = new LocatorTask(new Uri("https://geocode-api.arcgis.com/arcgis/rest/services/World/GeocodeServer"));
            locatorTask.ApiKey = Secrets.ARCGIS_KEY;

            var parameters = new GeocodeParameters();
            parameters.PreferredSearchLocation = TamuCenter;
            parameters.ResultAttributeNames.Add("Score");
            parameters.ResultAttributeNames.Add("Distance");

            var results = await locatorTask.GeocodeAsync(text, parameters);
            return Models.SearchResult.FromGeocodeResults(results);
        }

        private static Func<Feature, string> GetNameFunctionFromAttribute(string attributeName)
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
