using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        [System.ComponentModel.Bindable(true)]
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
        [System.ComponentModel.Bindable(true)]
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        private string shortName;
        [System.ComponentModel.Bindable(true)]
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

        public List<PatternElement> Stops { get; } = new List<PatternElement>();
        public List<PatternElement> DetailedPattern { get; } = new List<PatternElement>();
        public async Task<List<PatternElement>> GetDetailedPatternAsync()
        {
            if (DetailedPattern == null || DetailedPattern.Count <= 0)
            {
                foreach (PatternElement p in await TamuBusFeedApi.GetPattern(ShortName))
                {
                    p.Name = p.Name.Trim();
                    DetailedPattern.Add(p);

                    // Some of the names have leading whitespace for no reason
                    if (p.Stop != null)
                        Stops.Add(p);
                }
            }
            return DetailedPattern;
        }
    }
}
