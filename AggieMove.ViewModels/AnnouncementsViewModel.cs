using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
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
        }

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
        public IAsyncRelayCommand OpenInBrowserCommand { get; }

        public async Task LoadAnnouncementsAsync()
        {
            Feed = await TamuBusFeedApi.GetAnnouncements();
        }
    }
}
