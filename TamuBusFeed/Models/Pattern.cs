using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace TamuBusFeed.Models
{
    public class Pattern : ObservableObject
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

        private Direction direction;
        public Direction Direction
        {
            get => direction;
            set => SetProperty(ref direction, value);
        }

        private Info lineInfo;
        public Info LineInfo
        {
            get => lineInfo;
            set => SetProperty(ref lineInfo, value);
        }

        private Info timePointInfo;
        public Info TimePointInfo
        {
            get => timePointInfo;
            set => SetProperty(ref timePointInfo, value);
        }

        private Info busStopInfo;
        public Info BusStopInfo
        {
            get => busStopInfo;
            set => SetProperty(ref busStopInfo, value);
        }

        private bool isDisplay;
        public bool IsDisplay
        {
            get => isDisplay;
            set => SetProperty(ref isDisplay, value);
        }

        private bool isPrimary;
        public bool IsPrimary
        {
            get => isPrimary;
            set => SetProperty(ref isPrimary, value);
        }

    }
}
