using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace TamuBusFeed.Models
{
    public class Info : ObservableObject
    {
        private string color;
        public string Color
        {
        	get => color;
        	set => SetProperty(ref color, value);
        }

        private int type;
        public int Type
        {
        	get => type;
        	set => SetProperty(ref type, value);
        }

        // TODO: Make this an enum?
        private int symbol;
        public int Symbol
        {
        	get => symbol;
        	set => SetProperty(ref symbol, value);
        }

        private int size;
        public int Size
        {
        	get => size;
        	set => SetProperty(ref size, value);
        }

    }
}
