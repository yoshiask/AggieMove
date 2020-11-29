using System;
using Windows.UI.Xaml.Data;

namespace AggieMove.Converters
{
    public class DrawingColorToWinUIColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return Windows.UI.Colors.Transparent;

            var drawingColor = (System.Drawing.Color)value;
            return Windows.UI.Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return null;

            var winuiColor = (Windows.UI.Color)value;
            return System.Drawing.Color.FromArgb(winuiColor.A, winuiColor.R, winuiColor.G, winuiColor.B);
        }
    }
}
