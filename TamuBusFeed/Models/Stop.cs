using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace TamuBusFeed.Models
{
    public class Stop : ObservableObject
    {
        private string key;
        public string Key
        {
            get => key;
            set => SetProperty(ref key, value);
        }

        private int rank;
        public int Rank
        {
            get => rank;
            set => SetProperty(ref rank, value);
        }

        private string name;
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        private string stopCode;
        public string StopCode
        {
            get => stopCode;
            set => SetProperty(ref stopCode, value);
        }

        private bool isTimePoint;
        public bool IsTimePoint
        {
            get => isTimePoint;
            set => SetProperty(ref isTimePoint, value);
        }
    }
}
