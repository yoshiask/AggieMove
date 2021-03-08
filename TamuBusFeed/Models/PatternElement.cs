using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace TamuBusFeed.Models
{
    public class PatternElement : ObservableObject
    {
        private string key;
        public string Key
        {
            get => key;
            set => SetProperty(ref key, value);
        }

        private string name;
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        private string description;
        public string Description
        {
            get => description;
            set => SetProperty(ref description, value);
        }

        private int rank;
        public int Rank
        {
            get => rank;
            set => SetProperty(ref rank, value);
        }

        private double longitude;
        // TODO: Let Transportation Services know that this property is misspelled in the API
        [Newtonsoft.Json.JsonProperty("Longtitude")]
        public double Longitude
        {
            get => longitude;
            set => SetProperty(ref longitude, value);
        }

        private double latitude;
        public double Latitude
        {
            get => latitude;
            set => SetProperty(ref latitude, value);
        }

        private int pointTypeCode;
        public int PointTypeCode
        {
            get => pointTypeCode;
            set => SetProperty(ref pointTypeCode, value);
        }

        private int routeHeaderRank;
        public int RouteHeaderRank
        {
            get => routeHeaderRank;
            set => SetProperty(ref routeHeaderRank, value);
        }

        private Stop stop;
        public Stop Stop
        {
            get => stop;
            set => SetProperty(ref stop, value);
        }

    }
}
