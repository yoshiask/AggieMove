using AggieMove.Helpers;
using AggieMove.ViewModels;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Navigation;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Core;
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
    public sealed partial class DirectionsView : Page
    {
        private RouteTracker _tracker;
        private RouteResult _result;
        private Route _route;

        // List of driving directions for the route.
        private IReadOnlyList<DirectionManeuver> _directionsList;

        // Speech synthesizer to play voice guidance audio.
        private SpeechSynthesizer _speechSynthesizer = new();
        private MediaElement _mediaElement = new();

        // Graphics to show progress along the route.
        private Graphic _routeAheadGraphic;
        private Graphic _routeTraveledGraphic;

        public DirectionsView()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is not NavigateViewModel vm)
                return;

            _route = vm.Routes[vm.SelectedRouteIndex];
            _result = vm.Solve;

            await MainMapView.LoadMap();

            // Add graphics for the stops.
            SimpleMarkerSymbol stopSymbol = new(SimpleMarkerSymbolStyle.Diamond, Color.OrangeRed, 20);
            foreach (var stop in _route.Stops)
                MapGraphics.Graphics.Add(new Graphic(stop.Geometry, stopSymbol));

            // Create a graphic (with a dashed line symbol) to represent the route.
            _routeAheadGraphic = new Graphic(_route.RouteGeometry) { Symbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Color.BlueViolet, 5) };

            // Create a graphic (solid) to represent the route that's been traveled (initially empty).
            _routeTraveledGraphic = new Graphic { Symbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.LightBlue, 3) };

            // Add the route graphics to the map view.
            MapGraphics.Graphics.Add(_routeAheadGraphic);
            MapGraphics.Graphics.Add(_routeTraveledGraphic);

            // Set the map viewpoint to show the entire route.
            await MainMapView.SetViewpointGeometryAsync(_route.RouteGeometry, 100);

            // Get the directions for the route.
            _directionsList = _route.DirectionManeuvers;
            DirectionsListView.ItemsSource = _directionsList;

            _tracker = new(_result, vm.SelectedRouteIndex, true);
            _tracker.NewVoiceGuidance += SpeakDirection;

            // Handle route tracking status changes.
            _tracker.TrackingStatusChanged += TrackingStatusUpdated;

            // Turn on navigation mode for the map view.
            MainMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;
            MainMapView.LocationDisplay.AutoPanModeChanged += AutoPanModeChanged;

            // Add a data source for the location display.
            MainMapView.LocationDisplay.DataSource = new RouteTrackerLocationDataSource(_tracker, new SystemLocationDataSource());

            // Enable the location display (this wil start the location data source).
            MainMapView.LocationDisplay.IsEnabled = true;
        }

        private async void TrackingStatusUpdated(object sender, RouteTrackerTrackingStatusChangedEventArgs e)
        {
            TrackingStatus status = e.TrackingStatus;

            // Start building a status message for the UI.
            System.Text.StringBuilder statusMessageBuilder = new("Route Status:\r\n");

            // Check the destination status.
            if (status.DestinationStatus == DestinationStatus.NotReached || status.DestinationStatus == DestinationStatus.Approaching)
            {
                statusMessageBuilder.AppendLine("Distance remaining: " +
                                            status.RouteProgress.RemainingDistance.DisplayText + " " +
                                            status.RouteProgress.RemainingDistance.DisplayTextUnits.PluralDisplayName);

                statusMessageBuilder.AppendLine("Time remaining: " +
                                                status.RouteProgress.RemainingTime.ToString(@"hh\:mm\:ss"));

                if (status.CurrentManeuverIndex + 1 < _directionsList.Count)
                {
                    statusMessageBuilder.AppendLine("Next direction: " + _directionsList[status.CurrentManeuverIndex + 1].DirectionText);
                }

                // Set geometries for progress and the remaining route.
                _routeAheadGraphic.Geometry = status.RouteProgress.RemainingGeometry;
                _routeTraveledGraphic.Geometry = status.RouteProgress.TraversedGeometry;
            }
            else if (status.DestinationStatus == DestinationStatus.Reached)
            {
                statusMessageBuilder.AppendLine("Destination reached.");

                // Set the route geometries to reflect the completed route.
                _routeAheadGraphic.Geometry = null;
                _routeTraveledGraphic.Geometry = status.RouteResult.Routes[0].RouteGeometry;

                // Navigate to the next stop (if there are stops remaining).
                if (status.RemainingDestinationCount > 1)
                {
                    await _tracker.SwitchToNextDestinationAsync();
                }
                else
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        // Stop the simulated location data source.
                        MainMapView.LocationDisplay.DataSource.StopAsync();
                    });
                }
            }

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Show the status information in the UI.
                MessagesTextBlock.Text = statusMessageBuilder.ToString();
            });
        }

        private async void SpeakDirection(object sender, RouteTrackerNewVoiceGuidanceEventArgs e)
        {
            // Generate the audio stream for the voice guidance.
            SpeechSynthesisStream stream = await _speechSynthesizer.SynthesizeTextToStreamAsync(e.VoiceGuidance.Text);

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Play the audio stream.
                _mediaElement.SetSource(stream, stream.ContentType);
                _mediaElement.Play();
            });
        }

        private void AutoPanModeChanged(object sender, LocationDisplayAutoPanMode e)
        {
            // Turn the recenter button on or off when the location display changes to or from navigation mode.
            //RecenterButton.IsEnabled = e != LocationDisplayAutoPanMode.Navigation;
        }

        private void RecenterButton_Click(object sender, RoutedEventArgs e)
        {
            // Change the mapview to use navigation mode.
            MainMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            // Stop the speech synthesizer.
            _mediaElement.Stop();
            _speechSynthesizer.Dispose();

            // Stop the tracker.
            if (_tracker != null)
            {
                _tracker.TrackingStatusChanged -= TrackingStatusUpdated;
                _tracker.NewVoiceGuidance -= SpeakDirection;
                _tracker = null;
            }

            // Stop the location data source.
            MainMapView.LocationDisplay?.DataSource?.StopAsync();
        }
    }
}
