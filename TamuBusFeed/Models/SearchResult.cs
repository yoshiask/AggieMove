using Esri.ArcGISRuntime.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TamuBusFeed.Models
{
    public class SearchResult
    {
        public string Name { get; }
        public MapPoint Point { get; }

        public SearchResult(string name, MapPoint point)
        {
            Name = name;
            Point = point;
        }

        public SearchResult(string name, double lat, double lon) : this(name, new MapPoint(lat, lon))
        {

        }

        public static SearchResult FromFeature(Esri.ArcGISRuntime.Data.Feature feature,
            Func<Esri.ArcGISRuntime.Data.Feature, string> nameFactory)
        {
            return FromFeature(feature, nameFactory(feature));
        }

        public static SearchResult FromFeature(Esri.ArcGISRuntime.Data.Feature feature, string name)
        {
            return new SearchResult(name, feature.Geometry.Extent.GetCenter());
        }

        public static IEnumerable<SearchResult> FromFeatureQueryResult(Esri.ArcGISRuntime.Data.FeatureQueryResult fqr,
            Func<Esri.ArcGISRuntime.Data.Feature, string> nameFactory)
        {
            return fqr.Where(f => f?.Geometry?.Extent != null)
                .Select(f => FromFeature(f, nameFactory(f)));
        }

        public static SearchResult FromGeocodeResult(Esri.ArcGISRuntime.Tasks.Geocoding.GeocodeResult geo)
        {
            return new SearchResult(geo.Label, geo.DisplayLocation);
        }

        public static IEnumerable<SearchResult> FromGeocodeResults(IEnumerable<Esri.ArcGISRuntime.Tasks.Geocoding.GeocodeResult> geos)
        {
            return geos.Select(FromGeocodeResult);
        }
    }
}
