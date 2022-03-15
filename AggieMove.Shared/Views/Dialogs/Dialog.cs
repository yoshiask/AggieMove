using AggieMove.Services;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AggieMove.Views.Dialogs
{
    public abstract partial class Dialog<TViewModel> : ContentDialog, IDialog
    {
        public Dialog(object parameter)
        {
            Parameter = parameter;
        }

        protected object Parameter { get; }

        public TViewModel ViewModel
        {
            get => (TViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel), typeof(TViewModel), typeof(Dialog<TViewModel>), new PropertyMetadata(null));

        public object Result
        {
            get => (object)GetValue(ResultProperty);
            set => SetValue(ResultProperty, value);
        }
        public static readonly DependencyProperty ResultProperty = DependencyProperty.Register(
            nameof(Result), typeof(object), typeof(Dialog<TViewModel>), new PropertyMetadata(null));

        public object GetResult() => Result;

        public new async Task<object> ShowAsync() => await base.ShowAsync();
    }
}
