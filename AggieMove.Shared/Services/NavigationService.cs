﻿using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace AggieMove.Services
{
    public class NavigationService : INavigationService
    {
        public const string ASSEMBLY_FRAGMENT = "AggieMove.Views.";

        public Frame CurrentFrame { get; set; }

        public void Navigate(Type page)
        {
            CurrentFrame.Navigate(page);
        }

        public void Navigate(Type page, object parameter)
        {
            CurrentFrame.Navigate(page, parameter);
        }

        public void Navigate(string page)
        {
            Type type = Type.GetType(ASSEMBLY_FRAGMENT + page) ?? typeof(Views.NotImplementedPage);
            Navigate(type);
        }

        public void Navigate(string page, object parameter)
        {
            Type type = Type.GetType(ASSEMBLY_FRAGMENT + page);
            Navigate(type, parameter);
        }

        public void NavigateToSettingsPage(SettingsPages page)
        {
            CurrentFrame.Navigate(typeof(Views.SettingsPage), page);
        }

        public Task<DialogResult> ShowDialog(string page, object parameter = null)
        {
            Type type = Type.GetType(ASSEMBLY_FRAGMENT + page);
            return ShowDialog(type, parameter);
        }

        public async Task<DialogResult> ShowDialog(Type page, object parameter = null)
        {
            var inst = (IDialog)Activator.CreateInstance(page, parameter);
            var buttonResult =  await inst.ShowAsync();
            return new DialogResult((DialogButtonResult)buttonResult, inst.GetResult());
        }

        public void GoBack()
        {
            CurrentFrame.GoBack();
        }

        public bool TryGoBack()
        {
            if (CurrentFrame.CanGoBack)
            {
                GoBack();
                return true;
            }
            return false;
        }

        public async Task<bool> OpenInBrowser(string url)
        {
            // Wrap in a try-catch block in order to prevent the
            // app from crashing from invalid links.
            // (specifically from project badges)
            try
            {
                return await OpenInBrowser(new Uri(url));
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> OpenInBrowser(Uri uri)
        {
            return await Windows.System.Launcher.LaunchUriAsync(uri);
        }
    }
}
