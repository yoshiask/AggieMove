using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace TamuBusFeed.Models
{
    public class Direction : ObservableObject
    {
        private string key;
        public string Key
        {
            get => key;
            set => SetProperty(ref key, value);
        }

        private string name;
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }
    }
}
