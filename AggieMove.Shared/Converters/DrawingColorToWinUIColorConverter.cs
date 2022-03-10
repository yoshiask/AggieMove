using AggieMove.Helpers;
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
            return ColorHelper.ToWinUIColor(drawingColor); ;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return null;

            var winuiColor = (Windows.UI.Color)value;
            return ColorHelper.ToDrawingColor(winuiColor);
        }
    }
}
