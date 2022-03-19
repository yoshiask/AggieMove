using AggieMove.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TamuBusFeed;
using TamuBusFeed.Models;

namespace AggieMove.ViewModels
{
    public class AnnouncementsViewModel : ObservableRecipient
    {
        public AnnouncementsViewModel()
        {
            LoadAnnouncementsCommand = new AsyncRelayCommand(LoadAnnouncementsAsync);
            OpenAnnouncementCommand = new AsyncRelayCommand(OpenAnnouncementAsync);
        }

        /// <summary>
        /// The <see cref="INavigationService"/> instance currently in use.
        /// </summary>
        private readonly INavigationService NavigationService = Ioc.Default.GetRequiredService<INavigationService>();

        private AnnouncementFeed _Feed;
        public AnnouncementFeed Feed
        {
            get => _Feed;
            set => SetProperty(ref _Feed, value);
        }

        private AnnouncementItem _SelectedAnnouncement;
        public AnnouncementItem SelectedAnnouncement
        {
            get => _SelectedAnnouncement;
            set => SetProperty(ref _SelectedAnnouncement, value);
        }

        /// <summary>
        /// Gets the <see cref="IAsyncRelayCommand"/> instance responsible for loading announcements.
        /// </summary>
        public IAsyncRelayCommand LoadAnnouncementsCommand { get; }

        /// <summary>
        /// Gets the <see cref="IAsyncRelayCommand"/> instance responsible for navigating to the announcement's URL (if specified).
        /// </summary>
        public IAsyncRelayCommand OpenAnnouncementCommand { get; }

        public async Task LoadAnnouncementsAsync()
        {
            Feed = await TamuBusFeedApi.GetAnnouncements();
        }

        public async Task OpenAnnouncementAsync()
        {
            if (SelectedAnnouncement?.Links.Count >= 1 && SelectedAnnouncement.Links[0] != null)
            {
                string url = SelectedAnnouncement.Links[0].Uri;
                if (!url.StartsWith("http"))
                    url = "https:" + url;

                await NavigationService.OpenInBrowser(url);
                SelectedAnnouncement = null;
            }
        }
    }
}
