using System;
using System.Threading.Tasks;

namespace AggieMove.Services
{
    public interface INavigationService
    {
        public void Navigate(Type page);

        public void Navigate(Type page, object parameter);

        public void Navigate(string page);

        public void Navigate(string page, object parameter);

        public void NavigateToSettingsPage(SettingsPages page);

        public Task<DialogResult> ShowDialog(string page, object parameter = null);

        public Task<DialogResult> ShowDialog(Type page, object parameter = null);

        public void GoBack();

        public bool TryGoBack();

        public Task<bool> OpenInBrowser(string url);

        public Task<bool> OpenInBrowser(Uri uri);
    }

    public enum SettingsPages
    {
        General,
        About,
        Debug
    }
}
