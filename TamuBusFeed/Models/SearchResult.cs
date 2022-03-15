using Esri.ArcGISRuntime.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TamuBusFeed.Models
{
    public class SearchResult
    {
        /// <summary>
        /// Display name, typically only the title of the building/city/region.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The location.
        /// </summary>
        public MapPoint Point { get; }

        /// <summary>
        /// The full address of the location.
        /// </summary>
        public string Address { get; }

        public SearchResult(string name, MapPoint point, string address = null)
        {
            Name = name;
            Point = point;
            Address = address;
        }

        public SearchResult(string name, double lat, double lon, string address = null)
            : this(name, new MapPoint(lat, lon), address)
        {

        }

        public static SearchResult FromFeature(Esri.ArcGISRuntime.Data.Feature feature,
            Func<Esri.ArcGISRuntime.Data.Feature, string> nameFactory,
            Func<Esri.ArcGISRuntime.Data.Feature, string> addressFactory)
        {
            return FromFeature(feature, nameFactory(feature), addressFactory(feature));
        }

        public static SearchResult FromFeature(Esri.ArcGISRuntime.Data.Feature feature, string name, string address)
        {
            return new SearchResult(name, feature.Geometry.Extent.GetCenter(), address);
        }

        public static IEnumerable<SearchResult> FromFeatureQueryResult(Esri.ArcGISRuntime.Data.FeatureQueryResult fqr,
            Func<Esri.ArcGISRuntime.Data.Feature, string> nameFactory,
            Func<Esri.ArcGISRuntime.Data.Feature, string> addressFactory = null)
        {
            addressFactory ??= f => null;
            return fqr.Where(f => f?.Geometry?.Extent != null)
                .Select(f => FromFeature(f, nameFactory, addressFactory));
        }

        public static SearchResult FromGeocodeResult(Esri.ArcGISRuntime.Tasks.Geocoding.GeocodeResult geo)
        {
            return new SearchResult(geo.Label, geo.DisplayLocation, geo.Attributes["Place_addr"]?.ToString());
        }

        public static IEnumerable<SearchResult> FromGeocodeResults(IEnumerable<Esri.ArcGISRuntime.Tasks.Geocoding.GeocodeResult> geos)
        {
            return geos.Select(FromGeocodeResult);
        }
    }
}
