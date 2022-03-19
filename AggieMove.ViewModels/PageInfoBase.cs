using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace AggieMove.ViewModels
{
    public class PageInfoBase : ObservableObject
    {
        public PageInfoBase() { }

        public PageInfoBase(string title, string subhead, string glyph)
        {
            Title = title;
            Subhead = subhead;
            Glyph = glyph;
        }

        private string _Title;
        public string Title
        {
            get => _Title;
            set => SetProperty(ref _Title, value);
        }

        private string _Subhead;
        public string Subhead
        {
            get => _Subhead;
            set => SetProperty(ref _Subhead, value);
        }

        private string _Glyph;
        public string Glyph
        {
            get => _Glyph;
            set => SetProperty(ref _Glyph, value);
        }

        private string _PageType;
        public string PageType
        {
            get => _PageType;
            set => SetProperty(ref _PageType, value);
        }

        private string _Path;
        public string Path
        {
            get => _Path;
            set => SetProperty(ref _Path, value);
        }

        private string _Tooltip;
        public string Tooltip
        {
            get => _Tooltip;
            set => SetProperty(ref _Tooltip, value);
        }

        private bool _IsVisible;
        public bool IsVisible
        {
            get => _IsVisible;
            set => SetProperty(ref _IsVisible, value);
        }

        // Derived properties
        public string Protocol => "aggiemove://" + Path;
        public Uri IconAsset => new Uri("ms-appx:///Assets/Icons/" + Path + ".png");
    }
}
