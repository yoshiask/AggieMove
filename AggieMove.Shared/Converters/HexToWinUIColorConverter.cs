using AggieMove.Helpers;
using System;
using Windows.UI.Xaml.Data;

namespace AggieMove.Converters
{
    public class HexToWinUIColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return Windows.UI.Colors.Transparent;

            return ColorHelper.ToWinUIColor(ColorHelper.ParseHexColor((string)value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
