using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;

namespace TamuBusFeed.Models
{
    public class AnnouncementFeed : ObservableObject
    {
        private AnnouncementText description;
        public AnnouncementText Description
        {
            get => description;
            set => SetProperty(ref description, value);
        }

        private string id;
        public string Id
        {
            get => id;
            set => SetProperty(ref id, value);
        }

        private string imageUrl;
        public string ImageUrl
        {
            get => imageUrl;
            set => SetProperty(ref imageUrl, value);
        }

        private ObservableCollection<AnnouncementItem> items;
        public ObservableCollection<AnnouncementItem> Items
        {
            get => items;
            set => SetProperty(ref items, value);
        }

        private DateTimeOffset lastUpdatedTime;
        public DateTimeOffset LastUpdatedTime
        {
            get => lastUpdatedTime;
            set => SetProperty(ref lastUpdatedTime, value);
        }

        private AnnouncementText title;
        public AnnouncementText Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        private ObservableCollection<AnnouncementLink> links;
        public ObservableCollection<AnnouncementLink> Links
        {
            get => links;
            set => SetProperty(ref links, value);
        }
    }
}
