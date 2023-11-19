using AggieMove.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TamuBusFeed;
using TamuBusFeed.Models;

namespace AggieMove.ViewModels;

public partial class ExploreViewModel : ObservableObject
{
    public ExploreViewModel()
    {
        LoadRoutesCommand = new AsyncRelayCommand(LoadRoutesAsync);
        ViewRouteCommand = new RelayCommand(ViewRoute);
    }

    /// <summary>
    /// The <see cref="INavigationService"/> instance currently in use.
    /// </summary>
    private readonly INavigationService NavigationService = Ioc.Default.GetRequiredService<INavigationService>();
    private readonly TamuBusFeedApi Api = Ioc.Default.GetRequiredService<TamuBusFeedApi>();

    [System.ComponentModel.Bindable(true)]
    public ObservableCollection<RouteViewModel> Routes { get; } = new ObservableCollection<RouteViewModel>();

    [System.ComponentModel.Bindable(true)]
    [ObservableProperty]
    private RouteViewModel _selectedRoute;

    [System.ComponentModel.Bindable(true)]
    /// <summary>
    /// Gets the <see cref="IAsyncRelayCommand"/> instance responsible for loading posts.
    /// </summary>
    public IAsyncRelayCommand LoadRoutesCommand { get; }

    [System.ComponentModel.Bindable(true)]
    /// <summary>
    /// Gets the <see cref="IAsyncRelayCommand"/> instance responsible for showing route details.
    /// </summary>
    public IRelayCommand ViewRouteCommand { get; }

    public async Task LoadRoutesAsync()
    {
        Routes.Clear();
        await foreach (var r in Api.GetAllRoutes())
            Routes.Add(new RouteViewModel(r));
    }

    public void ViewRoute()
    {
        NavigationService.Navigate("RouteView", SelectedRoute);
    }
}
