using AggieMove.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AggieMove
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Shell : Page
    {
        private Services.NavigationService NavService { get; } = Ioc.Default.GetService<Services.INavigationService>() as Services.NavigationService;

        public Shell()
        {
            this.InitializeComponent();

            NavService.CurrentFrame = MainFrame;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Tuple<Type, object> launchInfo && launchInfo.Item1 != null)
                NavService.Navigate(launchInfo.Item1, launchInfo.Item2);

            base.OnNavigatedTo(e);
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            MainNav.IsBackEnabled = MainFrame.CanGoBack;
            try
            {
                // Update the NavView when the frame navigates on its own.
                // This is in a try-catch block so that I don't have to do a dozen
                // null checks.
                var page = ShellViewModel.Pages.Find((info) =>
                {
                    Type pageType = Type.GetType("AggieMove.Views." + info.PageType);
                    return pageType == e.SourcePageType;
                });
                if (page == null)
                {
                    MainNav.SelectedItem = null;
                    return;
                }
                MainNav.SelectedItem = MainNav.MenuItems.ToList().Find((obj) => (obj as NavigationViewItem).Content.ToString() == page.Title);
            }
            catch
            {
                MainNav.SelectedItem = null;
            }
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                NavService.NavigateToSettingsPage(Services.SettingsPages.General);
                return;
            }

            if (!(args.SelectedItem is NavigationViewItem navItem))
            {
                NavService.Navigate(typeof(Views.ExploreView));
                return;
            }

            PageInfoBase pageInfo = ShellViewModel.Pages.Find((info) => info.Title == navItem.Content.ToString());
            if (pageInfo == null)
            {
                NavService.Navigate(typeof(Views.ExploreView));
            }
            else
            {
                NavService.Navigate(pageInfo.PageType);
            }
        }
    }
}
