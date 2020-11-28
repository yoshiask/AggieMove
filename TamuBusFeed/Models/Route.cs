using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace TamuBusFeed.Models
{
    public class Route : ObservableObject
    {
        private Group group;
        public Group Group
        {
            get => group;
            set => SetProperty(ref group, value);
        }

        private string icon;
        public string Icon
        {
            get => icon;
            set => SetProperty(ref icon, value);
        }

        private TimeTableOverride timeTableOverride;
        public TimeTableOverride TimeTableOverride
        {
            get => timeTableOverride;
            set => SetProperty(ref timeTableOverride, value);
        }

        private bool webLink;
        public bool WebLink
        {
            get => webLink;
            set => SetProperty(ref webLink, value);
        }

        private string color;
        public string Color
        {
            get => color;
            set => SetProperty(ref color, value);
        }

        private Pattern pattern;
        public Pattern Pattern
        {
            get => pattern;
            set => SetProperty(ref pattern, value);
        }

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

        private string shortName;
        public string ShortName
        {
            get => shortName;
            set => SetProperty(ref shortName, value);
        }

        private string description;
        public string Description
        {
            get => description;
            set => SetProperty(ref description, value);
        }

        private RouteType routeType;
        public RouteType RouteType
        {
            get => routeType;
            set => SetProperty(ref routeType, value);
        }

    }
}
