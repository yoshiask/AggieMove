using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace TamuBusFeed.Models
{
    public class Group : ObservableObject
    {
        private string name;
        public string Name
        {
        	get => name;
        	set => SetProperty(ref name, value);
        }

        private int order;
        public int Order
        {
        	get => order;
        	set => SetProperty(ref order, value);
        }

        private bool isGameDay;
        public bool IsGameDay
        {
        	get => isGameDay;
        	set => SetProperty(ref isGameDay, value);
        }

    }
}
