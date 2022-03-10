using AggieMove.Helpers;
using Esri.ArcGISRuntime.Geometry;
using System;
using System.Linq;
using TamuBusFeed.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AggieMove.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RouteView : Page
    {
        public System.Drawing.Color DrawingColor { get; set; }

        public TextBlock CurrentTimeBlock { get; set; }

        public RouteView()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.SelectedRoute = e.Parameter as Route;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            MapHelper.LoadMap(MainMapView);

            await ViewModel.LoadPatternsAsync();
            DrawingColor = ColorHelper.ParseCSSColorAsDrawingColor(ViewModel.SelectedRoute.Color);

            bool hasRoutePoints = ViewModel.PatternElements.Count > 0;
            if (hasRoutePoints)
            {
                var geometry = MapHelper.DrawRouteAndStops(MainMapView, ViewModel, DrawingColor);
                _ = MainMapView.SetViewpointGeometryAsync(geometry.Geometry);
            }
            else
            {
                _ = MapHelper.SetViewpointToCurrentLocation(MainMapView, MapGraphics, Geolocator_PositionChanged, zoomToLocation: false);
            }

            // Show time table
            await ViewModel.LoadTimeTableAsync();
            Windows.UI.Xaml.Controls.Grid ttGrid;
            if (ViewModel.TimeTable != null)
            {
                ttGrid = TimeTableUIFactory.CreateGridFromTimeTable(ViewModel.TimeTable);
            }
            else
            {
                ttGrid = new Windows.UI.Xaml.Controls.Grid
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "Unable to load stop timing."
                        }
                    }
                };
            }
            TimeTablePresenter.Content = ttGrid;

            // Brings next nearest time into view
            if (ViewModel.TimeTable != null &&
                ttGrid.Children.FirstOrDefault(ui => ((ui as FrameworkElement)?.Tag as DateTimeOffset?) >= DateTimeOffset.Now) is TextBlock nowBlock)
            {
                CurrentTimeBlock = nowBlock;
                nowBlock.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 200, 0, 0));
            }

            base.OnNavigatedTo(e);
        }

        private async void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.SelectedPatternElement) && ViewModel.SelectedPatternElement != null)
            {
                PatternElement elem = ViewModel.SelectedPatternElement;
                var point = new MapPoint(elem.Longitude, elem.Latitude, MapHelper.BUS_ROUTES_SR);
                await MainMapView.SetViewpointCenterAsync(point);
                await MainMapView.SetViewpointScaleAsync(2000);
            }
        }

        private void Geolocator_PositionChanged(Windows.Devices.Geolocation.Geolocator sender, Windows.Devices.Geolocation.PositionChangedEventArgs args)
        {
            MapHelper.Geolocator_PositionChanged(MapGraphics.Graphics, Dispatcher, sender, args);
        }

        private async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is Pivot pivot && pivot.SelectedIndex == 1)
            {
                // If the Times tab hasn't been selected before,
                // we have to wait a bit before trying to scroll
                await System.Threading.Tasks.Task.Delay(10);
                
                // Scroll to current time
                var options = new BringIntoViewOptions
                {
                    VerticalAlignmentRatio = 0.5f,
                };
                CurrentTimeBlock.StartBringIntoView(options);
            }
        }
    }
}