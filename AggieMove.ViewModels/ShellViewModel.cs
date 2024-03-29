﻿using AggieMove.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AggieMove.ViewModels
{
    public class ShellViewModel : ObservableRecipient
    {
        public ShellViewModel()
        {
            //NavigateCommand = new RelayCommand<string>(Navigate);
            GoBackCommand = new RelayCommand(GoBack);
        }

        /// <summary>
        /// The <see cref="INavigationService"/> instance currently in use.
        /// </summary>
        private readonly INavigationService NavigationService = Ioc.Default.GetRequiredService<INavigationService>();

        // TODO: Is there a better way than to make this static?
        public static List<PageInfoBase> Pages = new List<PageInfoBase>
        {
            new PageInfoBase()
            {
                PageType = "NavigateView",
                Glyph = "\uE1D1", // Directions
                Title = "Navigate",
                Subhead = "to your destination",
                Tooltip = "Navigate to your destination"
            },

            new PageInfoBase()
            {
                PageType = "AnnouncementsView",
                Glyph = "\uE789", // Megaphone
                Title = "Announcements",
                Subhead = "",
                Tooltip = "View announcements from Transporation Services"
            },

            new PageInfoBase()
            {
                PageType = "DiscoverView",
                Glyph = "\uE1C4", // Map
                Title = "Discover",
                Subhead = "hotspots in your area",
                Tooltip = "Discover hotspots in your area",
            },

            new PageInfoBase()
            {
                PageType = "ExploreView",
                Glyph = "\uE1C3", // Street
                Title = "Explore",
                Subhead = "your transit options",
                Tooltip = "Explore your transit options"
            },

            new PageInfoBase()
            {
                PageType = "FavoritesView",
                Glyph = "\uE1CE", // Outline Star
                Title = "Favorites",
                Subhead = "View and manage your favorites",
                Tooltip = "View and manage your favorites",
            },
        };

        private ObservableCollection<PageInfoBase> _PageInfos = new ObservableCollection<PageInfoBase>(Pages);
        public ObservableCollection<PageInfoBase> PageInfos
        {
            get => _PageInfos;
            set => SetProperty(ref _PageInfos, value);
        }

        private PageInfoBase _SelectedPage;
        public PageInfoBase SelectedPage
        {
            get => _SelectedPage;
            set => SetProperty(ref _SelectedPage, value);
        }

        public bool ChangedByUserFlag = false;

        /// <summary>
        /// Gets the <see cref="IRelayCommand{string}"/> instance responsible for navigating.
        /// </summary>
        public IRelayCommand<string> NavigateCommand { get; }

        private void Navigate(string page)
        {

        }

        /// <summary>
        /// Gets the <see cref="IRelayCommand"/> instance responsible for navigating backward.
        /// </summary>
        public IRelayCommand GoBackCommand { get; }

        private void GoBack()
        {
            NavigationService.GoBack();
        }
    }
}
