using AggieMove.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using NavigationViewSelectionChangedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs;
using NavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
using NavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AggieMove
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Shell : Page
    {
        private Services.NavigationService NavService { get; } = Ioc.Default.GetService<Services.INavigationService>() as Services.NavigationService;
        private Services.SettingsService Settings { get; } = Ioc.Default.GetService<Services.SettingsService>();

        public Shell()
        {
            this.InitializeComponent();

            NavService.CurrentFrame = MainFrame;
            SizeChanged += Shell_SizeChanged;
            Loaded += Shell_Loaded;
        }

        private async void Shell_Loaded(object sender, RoutedEventArgs e)
        {
            await Settings.LoadAsync();
        }

        private void Shell_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 900)
            {
                VisualStateManager.GoToState(this, nameof(Normal), true);
            }
            else
            {
                VisualStateManager.GoToState(this, nameof(Compact), true);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Tuple<Type, object> launchInfo && launchInfo.Item1 != null)
                NavService.Navigate(launchInfo.Item1, launchInfo.Item2);
            else
                NavService.Navigate(typeof(Views.NavigateView));

            base.OnNavigatedTo(e);
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (ViewModel.ChangedByUserFlag)
            {
                ViewModel.ChangedByUserFlag = false;
                return;
            }

            try
            {
                // Update the NavView when the frame navigates on its own.
                // This is in a try-catch block so that I don't have to do a dozen
                // null checks.
                var pageIdx = ShellViewModel.Pages.FindIndex((info) =>
                {
                    Type pageType = Type.GetType(Services.NavigationService.ASSEMBLY_FRAGMENT + info.PageType);
                    return pageType == e.SourcePageType;
                });
                if (pageIdx >= 0)
                    ViewModel.SelectedPage = ShellViewModel.Pages[pageIdx];
            }
            catch
            {
                ViewModel.SelectedPage = null;
            }
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            ViewModel.ChangedByUserFlag = true;
            if (args.IsSettingsSelected)
            {
                NavService.NavigateToSettingsPage(Services.SettingsPages.General);
                return;
            }

            PageInfoBase pageInfo = null;

            if (args.SelectedItem is PageInfoBase pageInfoBase)
                pageInfo = pageInfoBase;
            else if (args.SelectedItem is NavigationViewItem navItem)
                pageInfo = ShellViewModel.Pages.Find((info) => info.Title == navItem.Content.ToString());

            if (pageInfo == null)
                NavService.Navigate(typeof(Views.NotImplementedPage));
            else
                NavService.Navigate(pageInfo.PageType);
        }
    }
}
