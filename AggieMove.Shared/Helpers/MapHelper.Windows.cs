#if WINDOWS_UWP

using System;
using System.Collections.Generic;
using System.Text;

namespace AggieMove.Helpers
{
    partial class MapHelper
    {
        public static Windows.UI.Xaml.UIElement CreateCallout(Esri.ArcGISRuntime.Data.GeoElement elem, GeoView view)
        {
            string title = elem.Attributes["Title"]?.ToString();
            string desc = elem.Attributes["Description"]?.ToString();

            var stack = new Windows.UI.Xaml.Controls.StackPanel
            {
                Children =
                {
                    new Windows.UI.Xaml.Controls.TextBlock
                    {
                        Text = title,
                        FontWeight = Windows.UI.Text.FontWeights.Bold
                    },
                    new Microsoft.Toolkit.Uwp.UI.Controls.MarkdownTextBlock
                    {
                        Text = desc,
                    },
                }
            };

            return stack;
        }
    }
}

#endif
