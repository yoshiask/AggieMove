using AggieMove.Helpers;
using AggieMove.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AggieMove.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NavigateView : Page
    {
        public NavigateView()
        {
            ViewModel = new NavigateViewModel();
            ViewModel.Stops.CollectionChanged += Stops_CollectionChanged;

            Loaded += Page_Loaded;
            this.InitializeComponent();
        }

        public NavigateViewModel ViewModel
        {
            get => (NavigateViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel), typeof(NavigateViewModel), typeof(NavigateView), new PropertyMetadata(null));

        private void Stops_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                MainMapView.GraphicsOverlays.Clear();
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    var stopGraphic = MapHelper.CreateRouteStop((Esri.ArcGISRuntime.Geometry.MapPoint)item, System.Drawing.Color.Blue);
                    MapGraphics.Graphics.Add(stopGraphic);
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MapHelper.LoadMap(MainMapView);
            MapHelper.SetViewpointToCurrentLocation(MainMapView, MapGraphics, Geolocator_PositionChanged, scale: 4000);
        }

        private void Geolocator_PositionChanged(Windows.Devices.Geolocation.Geolocator sender, Windows.Devices.Geolocation.PositionChangedEventArgs args)
        {
            MapHelper.Geolocator_PositionChanged(MapGraphics.Graphics, Dispatcher, sender, args);
        }
    }
}
