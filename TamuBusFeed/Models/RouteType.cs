using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace TamuBusFeed.Models
{
    public class RouteType : ObservableObject
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

        private string description;
        public string Description
        {
        	get => description;
        	set => SetProperty(ref description, value);
        }

    }
}
