using AggieMove.Helpers;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AggieMove
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static List<PageInfo> Pages = new List<PageInfo>
        {
            //new PageInfo()
            //{
            //    PageType = typeof(NavigateView),
            //    Icon = new SymbolIcon(Symbol.Directions),
            //    Title = "Navigate",
            //    Subhead = "to your destination",
            //    Tooltip = "Navigate to your destination"
            //},

            //new PageInfo()
            //{
            //    PageType = typeof(DiscoverView,
            //    Icon = new SymbolIcon(Symbol.Map),
            //    Title = "Discover",
            //    Subhead = "hotspots in your area",
            //    Tooltip = "Discover hotspots in your area",
            //},

            new PageInfo()
            {
                PageType = typeof(Views.ExploreView),
                Icon = new SymbolIcon(Symbol.Street),
                Title = "Explore",
                Subhead = "your transit options",
                Tooltip = "Explore your transit options"
            },

            //new PageInfo()
            //{
            //    PageType = typeof(FavoritesView),
            //    Icon = new SymbolIcon(Symbol.OutlineStar),
            //    Title = "Favorites",
            //    Subhead = "View and manage your favorites",
            //    Tooltip = "View and manage your favorites",
            //},
        };

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Tuple<Type, object> launchInfo && launchInfo.Item1 != null)
                NavigationManager.Navigate(launchInfo.Item1, launchInfo.Item2);

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
                var page = Pages.Find((info) => info.PageType == e.SourcePageType);
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
                NavigationManager.NavigateToSettings();
                return;
            }

            if (!(args.SelectedItem is NavigationViewItem navItem))
            {
                NavigationManager.NavigateToExplore();
                return;
            }

            PageInfo pageInfo = Pages.Find((info) => info.Title == navItem.Content.ToString());
            if (pageInfo == null)
            {
                NavigationManager.NavigateToExplore();
                return;
            }

            if (pageInfo != null && pageInfo.PageType.BaseType == typeof(Page))
                MainFrame.Navigate(pageInfo.PageType);
        }
    }
}
