using TamuBusFeed.Models;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AggieMove.Views
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class RouteView : Page
	{
		public RouteView()
		{
			this.InitializeComponent();
		}

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
			ViewModel.SelectedRoute = e.Parameter as Route;

            base.OnNavigatedTo(e);
        }
    }
}