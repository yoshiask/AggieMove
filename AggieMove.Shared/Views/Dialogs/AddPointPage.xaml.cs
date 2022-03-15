using AggieMove.ViewModels;
using Esri.ArcGISRuntime.Data;
using System.Linq;
using TamuBusFeed;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AggieMove.Views.Dialogs
{
    public abstract partial class AddPointDialog : Dialog<NavigateViewModel>
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
            this.PrimaryButtonClick += AddPointPage_PrimaryButtonClick;

            ViewModel = parameter as NavigateViewModel;
        }

        private void AddPointPage_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Close(SearchResultsList.Items.FirstOrDefault());
        }

        private async void SearchBox_TextChanged(object sender, TextChangedEventArgs args)
        {
            if (ViewModel.SearchCommand.IsRunning && ViewModel.SearchCommand.CanBeCanceled)
                ViewModel.SearchCommand.Cancel();

            await ViewModel.SearchCommand.ExecuteAsync(SearchBox.Text);
        }

        private void SearchResult_ItemClick(object sender, ItemClickEventArgs e)
        {
            Close(e.ClickedItem);
        }

        private void Close(object result)
        {
            ViewModel.SearchResults.Clear();
            Result = (TamuBusFeed.Models.SearchResult)result;
            Hide();
        }
    }
}
