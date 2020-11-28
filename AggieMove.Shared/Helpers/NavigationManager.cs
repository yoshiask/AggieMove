using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using AggieMove.Views;
using AggieMove.ViewModels;

namespace AggieMove.Helpers
{
    public static class NavigationManager
    {
        public static Tuple<Type, object> ParseProtocol(Uri ptcl)
        {
            Type destination = typeof(ExploreView);

            if (ptcl == null)
                return new Tuple<Type, object>(destination, null);

            string path;
            switch (ptcl.Scheme)
            {
                case "http":
                    path = ptcl.ToString().Remove(0, 23);
                    break;

                case "https":
                    path = ptcl.ToString().Remove(0, 24);
                    break;

                case "aggiemove":
                    path = ptcl.ToString().Remove(0, ptcl.Scheme.Length + 3);
                    break;

                default:
                    // Unrecognized protocol
                    return new Tuple<Type, object>(destination, null);
            }
            if (path.StartsWith("/"))
                path = path.Remove(0, 1);
            //var queryParams = System.Web.HttpUtility.ParseQueryString(ptcl.Query.Replace("\r", String.Empty).Replace("\n", String.Empty));

            PageInfoBase pageInfo = ShellViewModel.Pages.Find(p => p.Path == path.Split('/', StringSplitOptions.RemoveEmptyEntries)[0]);
            destination = pageInfo != null ? Type.GetType("AggieMove.Views." + pageInfo.PageType) : typeof(ExploreView);
            return new Tuple<Type, object>(destination, null);
        }
        public static Tuple<Type, object> ParseProtocol(string url)
        {
            return ParseProtocol(String.IsNullOrWhiteSpace(url) ? null : new Uri(url));
        }
    }

    public class PageInfo : PageInfoBase
    {
        public PageInfo() { }

        public PageInfo(string title, string subhead, IconElement icon)
        {
            Title = title;
            Subhead = subhead;
            Icon = icon;
        }

        public PageInfo(NavigationViewItem navItem)
        {
            Title = (navItem.Content == null) ? "" : navItem.Content.ToString();
            Icon = (navItem.Icon == null) ? new SymbolIcon(Symbol.Document) : navItem.Icon;
            Visibility = navItem.Visibility;

            var tooltip = ToolTipService.GetToolTip(navItem);
            Tooltip = (tooltip == null) ? "" : tooltip.ToString();
        }

        private IconElement _Icon;
        public IconElement Icon
        {
            get => _Icon;
            set => SetProperty(ref _Icon, value);
        }

        private Visibility _Visibility;
        public Visibility Visibility
        {
            get => _Visibility;
            set
            {
                SetProperty(ref _Visibility, value);
                IsVisible = value == Visibility.Visible;
            }
        }

        private bool _IsVisible;
        public new bool IsVisible
        {
            get => _IsVisible;
            set
            {
                SetProperty(ref _IsVisible, value);
                Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        // Derived properties
        public NavigationViewItem NavViewItem
        {
            get
            {
                var item = new NavigationViewItem()
                {
                    Icon = Icon,
                    Content = Title,
                    Visibility = Visibility
                };
                ToolTipService.SetToolTip(item, new ToolTip() { Content = Tooltip });

                return item;
            }
        }
    }

    public enum SettingsPages
    {
        General,
        About,
        Debug
    }
}