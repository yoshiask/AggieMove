using Esri.ArcGISRuntime.Geometry;
using System;
using System.Collections.Generic;

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

        public static IEnumerable<SearchResult> FromFeatureQueryResult(Esri.ArcGISRuntime.Data.FeatureQueryResult fqr,
            Func<Esri.ArcGISRuntime.Data.Feature, string> getName)
        {
            foreach (var feature in fqr)
                if (feature?.Geometry?.Extent != null)
                    yield return new SearchResult(getName(feature), feature.Geometry.Extent.GetCenter());
        }
    }
}
