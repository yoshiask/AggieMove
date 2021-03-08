using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AggieMove.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AnnouncementsView : Page
    {
        public AnnouncementsView()
        {
            this.InitializeComponent();
        }
        private void OnAnnouncementSelected(object sender, SelectionChangedEventArgs e)
        {
#if !NETFX_CORE
            // TODO: This is an ugly, pattern-breaking workaround because Uno fires the SelectionChanged event before
            // setting the selected item
            ViewModel.SelectedAnnouncement = e.AddedItems[0] as TamuBusFeed.Models.AnnouncementItem;
#endif
        }
    }
}
