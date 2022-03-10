using AggieMove.ViewModels;
using Esri.ArcGISRuntime.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TamuBusFeed;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AggieMove.Views.Dialogs
{
    public abstract class AddPointDialog : Dialog<NavigateViewModel>
    {
        public AddPointDialog(object parameter) : base(parameter)
        {
        }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddPointPage : AddPointDialog
    {
        public AddPointPage(object parameter) : base(parameter)
        {
            this.InitializeComponent();
            SearchBox.TextChanged += SearchBox_TextChanged;
            ViewModel = parameter as NavigateViewModel;
        }

        private async void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput || !args.CheckCurrent())
                return;

            var results = await TamuArcGisApi.QueryBuildings(sender.Text);
            sender.Items.Clear();
            foreach (var result in results)
            {
                sender.Items.Add(result);
            }
        }

        private async void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            Result = (Feature)(args.ChosenSuggestion ?? sender.Items.FirstOrDefault());
            Hide();
        }
    }
}
