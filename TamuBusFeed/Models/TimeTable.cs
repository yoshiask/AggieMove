using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TamuBusFeed.Models
{
    public class TimeTable : ObservableObject
    {
        private ObservableCollection<TimeStop> timeStops;
        public ObservableCollection<TimeStop> TimeStops
        {
            get => timeStops;
            set => SetProperty(ref timeStops, value);
        }
    }

    public class TimeStop : ObservableObject
    {
        private ObservableCollection<DateTimeOffset?> leaveTimes;
        public ObservableCollection<DateTimeOffset?> LeaveTimes
        {
            get => leaveTimes;
            set => SetProperty(ref leaveTimes, value);
        }

        private string name;
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        public IEnumerable<string> GetFormattedLeaveTimes() => leaveTimes.Select(t => t.HasValue ? t.Value.ToString("hh:mm tt") : "...");
    }
}
