using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using ProjectRunner.ServerAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectRunner.ViewModel
{
    public class HomePageViewModel : MyViewModel
    {
        private PRServer server;
        public HomePageViewModel(INavigationService n, PRServer s) : base(n)
        {
            server = s;
        }
        public RelayCommand TestCommand => new RelayCommand(() =>
        {
            //server.GoogleMaps.GetCoordinatesFromAddress("Santa Maria la Carità", "Via Visitazione", "290", "80050");
            navigation.NavigateTo(ViewModelLocator.SettingsPage);
        });
        public override void NavigatedToAsync(object parameter = null)
        {
            CheckLocationPermissionAsync();
        }
        private async Task CheckLocationPermissionAsync()
        {
            var locator = Plugin.Geolocator.CrossGeolocator.Current;
            if (!locator.IsGeolocationAvailable)
                return;
            await CheckPermissionAsync(Permission.Location);
        }
    }
}
