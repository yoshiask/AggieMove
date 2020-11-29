using System;
using Windows.UI.Xaml.Data;

namespace AggieMove.Converters
{
    public class CSSColorToWinUIColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return Windows.UI.Colors.Transparent;

            return Helpers.ColorHelper.ParseCSSColorAsWinUIColor((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
