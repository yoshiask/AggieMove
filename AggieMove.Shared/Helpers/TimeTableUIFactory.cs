using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TamuBusFeed.Models;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AggieMove.Helpers
{
    public static class TimeTableUIFactory
    {
        public static Grid CreateGridFromTimeTable(TimeTable timeTable)
        {
            Grid MainGrid = new Grid();

            // Create a column for each time stop
            for (int i = 0; i < timeTable.TimeStops.Count; i++)
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition());

            // Create rows for each bus round, plus one for headers
            int numRows = timeTable.TimeStops.Max(t => t.LeaveTimes.Count) + 1;
            for (int i = 0; i < numRows; i++)
                MainGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            for (int t = 0; t < timeTable.TimeStops.Count; t++)
            {
                TimeStop timeStop = timeTable.TimeStops[t];
                var header = new TextBlock()
                {
                    Padding = new Thickness(4),
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Text = timeStop.Name
                };
                MainGrid.Children.Add(header);
                Grid.SetColumn(header, t);

                int i = 1;
                foreach (string time in timeStop.GetFormattedLeaveTimes())
                {
                    var block = new TextBlock()
                    {
                        Padding = new Thickness(4),
                        Text = time,
                        Tag = time
                    };
                    MainGrid.Children.Add(block);
                    Grid.SetColumn(block, t);
                    Grid.SetRow(block, i++);
                }
            }
            return MainGrid;
        }
    }
}
