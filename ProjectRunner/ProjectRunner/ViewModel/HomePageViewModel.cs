using GalaSoft.MvvmLight.Command;
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
        public HomePageViewModel(PRServer s)
        {
            server = s;
        }
        public RelayCommand TestCommand => new RelayCommand(() =>
        {
            //server.GoogleMaps.GetCoordinatesFromAddress("Santa Maria la Carità", "Via Visitazione", "290", "80050");
        });
        public override void NavigatedToAsync(object parameter = null)
        {
            CheckLocationPermissionAsync();
        }
        private async Task CheckLocationPermissionAsync()
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);
                if (status != PermissionStatus.Granted)
                {

                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Location)) { }
                    
                    var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Location);
                    //Best practice to always check that the key exists
                    if (results.ContainsKey(Permission.Location))
                        status = results[Permission.Location];
                }
                /*
                if (status == PermissionStatus.Granted)
                {
                    var results = await CrossGeolocator.Current.GetPositionAsync(10000);
                    LabelGeolocation.Text = "Lat: " + results.Latitude + " Long: " + results.Longitude;
                }
                else if (status != PermissionStatus.Unknown)
                {
                    await DisplayAlert("Location Denied", "Can not continue, try again.", "OK");
                }
                */
            }
            catch (Exception ex)
            {

               // LabelGeolocation.Text = "Error: " + ex;
            }
        }
    }
}
