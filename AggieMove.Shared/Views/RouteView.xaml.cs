using AggieMove.Helpers;
using Esri.ArcGISRuntime.Geometry;
using System;
using System.Linq;
using System.Timers;
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
        private readonly Windows.UI.Xaml.Media.SolidColorBrush currentTimeHighlightBrush = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 200, 0, 0));
        private readonly Windows.UI.Xaml.Media.SolidColorBrush pastTimeHighlightBrush = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 125, 125, 125));
        private readonly BringIntoViewOptions currentTimeViewOptions = new BringIntoViewOptions
        {
            VerticalAlignmentRatio = 0.5f,
        };
        private readonly Timer clock;

        public System.Drawing.Color DrawingColor { get; private set; }
        public TextBlock CurrentTimeBlock { get; private set; }
        public Grid TimeTableGrid { get; private set; }

        public RouteView()
        {
            clock = new Timer();
            clock.Interval = 60 * 1000;     // One minute
            clock.Elapsed += Clock_Elapsed;

            this.InitializeComponent();

            MainMapView.LocationDisplay.IsEnabled = true;
            MainMapView.LocationDisplay.AutoPanMode = Esri.ArcGISRuntime.UI.LocationDisplayAutoPanMode.Recenter;
            MainMapView.GeoViewTapped += MainMapView_GeoViewTapped;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.SelectedRoute = e.Parameter as Route;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            await MapHelper.LoadMap(MainMapView);

            await ViewModel.LoadPatternsAsync();
            DrawingColor = ColorHelper.ParseCSSColorAsDrawingColor(ViewModel.SelectedRoute.Color);

            bool hasRoutePoints = ViewModel.PatternElements.Count > 0;
            if (hasRoutePoints)
            {
                var geometry = MapHelper.DrawRouteAndStops(MainMapView, ViewModel, DrawingColor);
                _ = MainMapView.SetViewpointGeometryAsync(geometry.Geometry);
            }

            // Show time table
            await ViewModel.LoadTimeTableAsync();
            if (ViewModel.TimeTable != null)
            {
                TimeTableGrid = TimeTableUIFactory.CreateGridFromTimeTable(ViewModel.TimeTable);
                UpdateCurrentTimeBlock();
                clock.Start();
            }
            else
            {
                TimeTableGrid = new Grid
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
            TimeTablePresenter.Content = TimeTableGrid;

            base.OnNavigatedTo(e);
        }

        private async void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.SelectedPatternElement) && ViewModel.SelectedPatternElement != null)
            {
                PatternElement elem = ViewModel.SelectedPatternElement;
                var point = new MapPoint(elem.Longitude, elem.Latitude, SpatialReferences.WebMercator);
                await MainMapView.SetViewpointCenterAsync(point);
                await MainMapView.SetViewpointScaleAsync(2000);
            }
        }

        private async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is Pivot pivot && pivot.SelectedIndex == 1)
            {
                // If the Times tab hasn't been selected before,
                // we have to wait a bit before trying to scroll
                await System.Threading.Tasks.Task.Delay(10);
                
                // Scroll to current time
                CurrentTimeBlock?.StartBringIntoView(currentTimeViewOptions);
            }
        }

        private void UpdateCurrentTimeBlock()
        {
            // Make sure a time isn't already highlighted
            if (CurrentTimeBlock != null)
            {
                CurrentTimeBlock.Foreground = pastTimeHighlightBrush;
            }

            // Brings next nearest time into view
            if (TimeTableGrid.Children.FirstOrDefault(ui => ((ui as FrameworkElement)?.Tag as DateTimeOffset?) >= DateTimeOffset.Now) is TextBlock nowBlock)
            {
                CurrentTimeBlock = nowBlock;
                nowBlock.Foreground = currentTimeHighlightBrush;
            }
        }

        private async void Clock_Elapsed(object sender, ElapsedEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, UpdateCurrentTimeBlock);
        }

        private async void MainMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            // Identify graphics using the screen tap.
            var screenPoint = e.Position;
            var resultGraphics = await MainMapView.IdentifyGraphicsOverlaysAsync(screenPoint, 10, false);

            // Show details in a callout for the first graphic identified (if any).
            if (resultGraphics != null && resultGraphics.Count > 0)
            {
                var poiGraphic = resultGraphics.FirstOrDefault()?.Graphics.FirstOrDefault();
                var callout = MapHelper.CreateCallout(poiGraphic);

                MapPoint calloutAnchor = poiGraphic.Geometry.GetClosestPoint(e.Location);
                MainMapView.ShowCalloutAt(calloutAnchor, callout);
            }
        }
    }
}