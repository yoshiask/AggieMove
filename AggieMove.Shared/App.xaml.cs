using AggieMove.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace AggieMove
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeLogging();

            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            //Uno.Material.Resources.Init(this, new ResourceDictionary() { Source = new Uri("ms-appx:///ColorPaletteOverride.xaml") });
            Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.SetLicense("runtimelite,1000,rud5976483922,none,GB2PMD17J06HZF3RE159");
            TamuBusFeed.TamuArcGisApi.ApiKey = Secrets.ARCGIS_KEY;

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                // Add support for accelerator keys. 
                // Listen to the window directly so the app responds
                // to accelerator keys regardless of which element has focus.
                Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += CoreDispatcher_AcceleratorKeyActivated;

                // Add support for system back requests. 
                SystemNavigationManager.GetForCurrentView().BackRequested += System_BackRequested;

                // Add support for mouse navigation buttons. 
                Window.Current.CoreWindow.PointerPressed += CoreWindow_PointerPressed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;

                // Register services
                Ioc.Default.ConfigureServices(
                    new ServiceCollection()
                    .AddSingleton<INavigationService, NavigationService>()
                    //.AddSingleton<ISettingsService, SettingsService>()
                    .BuildServiceProvider());
            }

            if (e.PrelaunchActivated == false)
            {
                Windows.ApplicationModel.Core.CoreApplication.EnablePrelaunch(true);
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(Shell), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }


        /// <summary>
        /// Invoked on every keystroke, including system keys such as Alt key combinations.
        /// Used to detect keyboard navigation between pages even when the page itself
        /// doesn't have focus.
        /// </summary>
        private void CoreDispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs e)
        {
            INavigationService NavigationService = Ioc.Default.GetRequiredService<INavigationService>();

            // When Alt+Left are pressed navigate back.
            // When Alt+Right are pressed navigate forward.
            if (e.EventType == CoreAcceleratorKeyEventType.SystemKeyDown
                && (e.VirtualKey == VirtualKey.Left || e.VirtualKey == VirtualKey.Right)
                && e.KeyStatus.IsMenuKeyDown == true
                && !e.Handled)
            {
                if (e.VirtualKey == VirtualKey.Left)
                {
                    e.Handled = NavigationService.TryGoBack();
                }
                else if (e.VirtualKey == VirtualKey.Right)
                {
                    // TODO: e.Handled = TryGoForward();
                }
            }
        }

        /// <summary>
        /// Handle system back requests.
        /// </summary>
        private void System_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (!e.Handled)
            {
                INavigationService NavigationService = Ioc.Default.GetRequiredService<INavigationService>();
                e.Handled = NavigationService.TryGoBack();
            }
        }

        /// <summary>
        /// Handle mouse back button.
        /// </summary>
        private void CoreWindow_PointerPressed(CoreWindow sender, PointerEventArgs e)
        {
            INavigationService NavigationService = Ioc.Default.GetRequiredService<INavigationService>();

            // For this event, e.Handled arrives as 'true', so invert the value.
            if (e.CurrentPoint.Properties.IsXButton1Pressed
                && e.Handled)
            {
                e.Handled = !NavigationService.TryGoBack();
            }
            else if (e.CurrentPoint.Properties.IsXButton2Pressed
                    && e.Handled)
            {
                //e.Handled = !TryGoForward();
            }
        }


        /// <summary>
        /// Configures global Uno Platform logging
        /// </summary>
        private static void InitializeLogging()
        {
            var factory = LoggerFactory.Create(builder =>
            {
#if __WASM__
                    builder.AddProvider(new global::Uno.Extensions.Logging.WebAssembly.WebAssemblyConsoleLoggerProvider());
#elif __IOS__
                    builder.AddProvider(new global::Uno.Extensions.Logging.OSLogLoggerProvider());
#elif NETFX_CORE
                    builder.AddDebug();
#else
                builder.AddConsole();
#endif

                // Exclude logs below this level
                builder.SetMinimumLevel(LogLevel.Information);

                // Default filters for Uno Platform namespaces
                builder.AddFilter("Uno", LogLevel.Warning);
                builder.AddFilter("Windows", LogLevel.Warning);
                builder.AddFilter("Microsoft", LogLevel.Warning);

                // Generic Xaml events
                // builder.AddFilter("Windows.UI.Xaml", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.VisualStateGroup", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.StateTriggerBase", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.UIElement", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.FrameworkElement", LogLevel.Trace );

                // Layouter specific messages
                // builder.AddFilter("Windows.UI.Xaml.Controls", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.Controls.Layouter", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.Controls.Panel", LogLevel.Debug );

                // builder.AddFilter("Windows.Storage", LogLevel.Debug );

                // Binding related messages
                // builder.AddFilter("Windows.UI.Xaml.Data", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.Data", LogLevel.Debug );

                // Binder memory references tracking
                // builder.AddFilter("Uno.UI.DataBinding.BinderReferenceHolder", LogLevel.Debug );

                // RemoteControl and HotReload related
                // builder.AddFilter("Uno.UI.RemoteControl", LogLevel.Information);

                // Debug JS interop
                // builder.AddFilter("Uno.Foundation.WebAssemblyRuntime", LogLevel.Debug );
            });

            global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;
        }
    }
}
