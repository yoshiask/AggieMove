using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace TamuBusFeed.Models
{
    public class TimeTableOverride : ObservableObject
    {
        private bool enabled;
        public bool Enabled
        {
            get => enabled;
            set => SetProperty(ref enabled, value);
        }

        private double before;
        public double Before
        {
            get => before;
            set => SetProperty(ref before, value);
        }

        private bool during;
        public bool During
        {
            get => during;
            set => SetProperty(ref during, value);
        }

        private double after;
        public double After
        {
            get => after;
            set => SetProperty(ref after, value);
        }
    }
}
