#if __ANDROID__

using Android.Widget;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using Uno.UI;

namespace AggieMove.Helpers
{
    partial class MapHelper
    {
        public static Android.Views.View CreateCallout(Esri.ArcGISRuntime.Data.GeoElement elem, GeoView view)
        {
            string title = elem.Attributes["Title"]?.ToString();
            string desc = elem.Attributes["Description"]?.ToString();

            var layout = new LinearLayout(view.Context)
            {
                Orientation = Orientation.Vertical
            };
            layout.AddChild(new TextView(view.Context)
            {
                Text = title,
            });
            layout.AddChild(new TextView(view.Context)
            {
                Text = desc,
            });

            return layout;
        }
    }
}

#endif
