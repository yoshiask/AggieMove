using AggieMove.ViewModels;
using Esri.ArcGISRuntime.Data;
using System.Linq;
using TamuBusFeed;
using Windows.UI.Xaml.Controls;

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

            var results = await TamuArcGisApi.SearchAsync(sender.Text);
            sender.Items.Clear();
            SearchResultsList.Items.Clear();
            foreach (var result in results)
            {
                sender.Items.Add(result);
                SearchResultsList.Items.Add(result);
            }
        }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            Close(args.ChosenSuggestion ?? sender.Items.FirstOrDefault());
        }

        private void SearchResult_ItemClick(object sender, ItemClickEventArgs e)
        {
            Close(e.ClickedItem);
        }

        private void Close(object result)
        {
            Result = (TamuBusFeed.Models.SearchResult)result;
            Hide();
        }
    }
}
