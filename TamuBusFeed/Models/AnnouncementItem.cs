using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;

namespace TamuBusFeed.Models
{
    public class AnnouncementItem : ObservableObject
    {
        private AnnouncementText title;
        public AnnouncementText Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        private object content;
        public object Content
        {
            get => content;
            set => SetProperty(ref content, value);
        }

        private AnnouncementText summary;
        public AnnouncementText Summary
        {
            get => summary;
            set => SetProperty(ref summary, value);
        }

        private string id;
        public string Id
        {
            get => id;
            set => SetProperty(ref id, value);
        }

        private DateTimeOffset publishDate;
        public DateTimeOffset PublishDate
        {
            get => publishDate;
            set => SetProperty(ref publishDate, value);
        }

        private ObservableCollection<AnnouncementLink> links;
        public ObservableCollection<AnnouncementLink> Links
        {
            get => links;
            set => SetProperty(ref links, value);
        }

        private ObservableCollection<AnnouncementCategory> categories;
        public ObservableCollection<AnnouncementCategory> Categories
        {
            get => categories;
            set => SetProperty(ref categories, value);
        }
    }

    public class AnnouncementText : ObservableObject
    {
        private string type;
        public string Type
        {
            get => type;
            set => SetProperty(ref type, value);
        }

        private string text;
        public string Text
        {
            get => text;
            set => SetProperty(ref text, value);
        }
    }

    public class AnnouncementCategory : ObservableObject
    {
        private string name;
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        private string label;
        public string Label
        {
            get => label;
            set => SetProperty(ref label, value);
        }

        private string scheme;
        public string Scheme
        {
            get => scheme;
            set => SetProperty(ref scheme, value);
        }
    }

    public class AnnouncementLink : ObservableObject
    {
        private string title;
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        private string baseUri;
        public string BaseUri
        {
            get => baseUri;
            set => SetProperty(ref baseUri, value);
        }

        private string uri;
        public string Uri
        {
            get => uri;
            set => SetProperty(ref uri, value);
        }

        private int length;
        public int Length
        {
            get => length;
            set => SetProperty(ref length, value);
        }

        private string mediaType;
        public string MediaType
        {
            get => mediaType;
            set => SetProperty(ref mediaType, value);
        }

        private string relationshipType;
        public string RelationshipType
        {
            get => relationshipType;
            set => SetProperty(ref relationshipType, value);
        }
    }
}
