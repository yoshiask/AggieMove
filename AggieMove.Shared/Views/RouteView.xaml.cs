using AggieMove.Helpers;
using AggieMove.ViewModels;
using Esri.ArcGISRuntime.Geometry;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Timers;
using TamuBusFeed;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AggieMove.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RouteView : Page
    {
        private readonly SolidColorBrush currentTimeHighlightBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 200, 0, 0));
        private readonly SolidColorBrush pastTimeHighlightBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 125, 125, 125));
        private readonly BringIntoViewOptions currentTimeViewOptions = new BringIntoViewOptions
        {
            VerticalAlignmentRatio = 0.5f,
        };
        private readonly Timer clock;

        public RouteViewModel ViewModel
        {
            get => (RouteViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel), typeof(RouteViewModel), typeof(RouteView), new PropertyMetadata(null));

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
            ViewModel = e.Parameter as RouteViewModel;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            await MainMapView.LoadMap();

            await ViewModel.LoadPatternsAsync();
            DrawingColor = ColorHelper.ParseHexColor(ViewModel.Pattern.Color);

            bool hasRoutePoints = (ViewModel.Pattern?.PatternPaths?.Count ?? 0) > 0;
            if (hasRoutePoints)
            {
                ViewModel.Graphic = MainMapView.DrawRouteAndStops(ViewModel, DrawingColor);
                _ = MainMapView.SetViewpointGeometryAsync(ViewModel.Graphic.Geometry);
            }

            // Show time table
            await ViewModel.LoadTimeTableAsync();
            if (ViewModel.TimeTables != null)
            {
                TimeTableGrid = TimeTableUIFactory.CreateGridFromTimeTable(ViewModel.TimeTables[0]);
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

            // Start watching for vehicle udpates
            ViewModel.StartWatchingVehicles(OnVehiclesUpdated);

            base.OnNavigatedTo(e);
        }

        private async void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.SelectedPatternPoint) && ViewModel.SelectedPatternPoint != null)
            {
                var path = ViewModel.SelectedPatternPoint;
                var point = new MapPoint(path.Longitude, path.Latitude, TamuArcGisApi.TamuSpatialReference);
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
            await MainMapView.HandleGeoViewTapped(e);
        }

        private async void OnVehiclesUpdated(object sender, NotifyCollectionChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate
            {
                UpdateVehicles(e.Action, e.NewItems?.Cast<VehicleViewModel>());
            });
        }

        private void UpdateVehicles(NotifyCollectionChangedAction action, IEnumerable<VehicleViewModel> newItems)
        {
            switch (action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (newItems != null)
                    {
                        foreach (var vehicle in newItems)
                        {
                            MainMapView.DrawVehicle(vehicle);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (newItems != null)
                    {
                        foreach (var vehicle in newItems)
                        {
                            MainMapView.RemoveVehicle(vehicle.Mentor.Key);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    {
                        MainMapView.ClearAllVehicleOverlays();
                    }
                    break;
            }
        }
    }
}