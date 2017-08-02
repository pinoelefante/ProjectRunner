using Acr.UserDialogs;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Plugin.Geolocator;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using ProjectRunner.ServerAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace ProjectRunner.ViewModel
{
    public class AddLocationViewModel : MyViewModel
    {
        private PRServer server;
        public AddLocationViewModel(INavigationService nav, PRServer s) : base(nav)
        {
            server = s;
        }
        public override void NavigatedToAsync(object parameter = null)
        {
            Cleanup();
        }
        public override void Cleanup()
        {
            AddressName = string.Empty;
            AddressStreet = string.Empty;
            AddressCity = string.Empty;
            AddressCivicNumber = string.Empty;
            AddressZipCode = string.Empty;
            AddressLatitude = 0;
            AddressLongitude = 0;
            UseGPS = false;
            base.Cleanup();
        }
        private string _mpName = string.Empty, _mpStreet, _mpCity, _mpZipCode;
        private string _mpCivicNumber;
        private double _mpLatitude, _mpLongitude;
        public string AddressName { get { return _mpName; } set { Set(ref _mpName, value); } }
        public string AddressStreet { get { return _mpStreet; } set { Set(ref _mpStreet, value); } }
        public string AddressCity { get { return _mpCity; } set { Set(ref _mpCity, value); } }
        public string AddressCivicNumber { get { return _mpCivicNumber; } set { Set(ref _mpCivicNumber, value); } }
        public string AddressZipCode { get { return _mpZipCode; } set { Set(ref _mpZipCode, value); } }
        public double AddressLatitude { get { return _mpLatitude; } set { Set(ref _mpLatitude, value); } }
        public double AddressLongitude { get { return _mpLongitude; } set { Set(ref _mpLongitude, value); } }
        public bool HasGPS { get { return CrossGeolocator.Current.IsGeolocationAvailable; } }

        private bool _useGps = false;
        public bool UseGPS
        {
            get { return _useGps; }
            set
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (value)
                    {
                        var locator = CrossGeolocator.Current;
                        if (!locator.IsGeolocationAvailable)
                        {
                            UserDialogs.Instance.Alert("You device does not support geolocation", "Location non available");
                            Set(ref _useGps, false);
                            return;
                        }
                        if (!locator.IsGeolocationEnabled)
                        {
                            UserDialogs.Instance.Alert("You have to turn on the location system", "Location non available");
                            Set(ref _useGps, false);
                            return;
                        }
                        try
                        {
                            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);
                            if (status != PermissionStatus.Granted)
                            {
                                Set(ref _useGps, false);
                                UserDialogs.Instance.Alert("You have to enable location in system settings", "Access to location");
                                return;
                            }
                            Set(ref _useGps, value);
                        }
                        catch
                        {
                            Set(ref _useGps, false);
                        }
                    }
                    else
                        Set(ref _useGps, value);
                });
                RaisePropertyChanged(() => UseGPS);
            }
        }
        private RelayCommand _addLocationCmd;
        public RelayCommand AddLocationCommand =>
            _addLocationCmd ??
            (_addLocationCmd = new RelayCommand(async () =>
            {
                if (string.IsNullOrEmpty(AddressName.Trim()))
                {
                    UserDialogs.Instance.Alert("You must give a name to the location");
                    return;
                }

                if (UseGPS)
                {
                    var locator = CrossGeolocator.Current;
                    if (locator.IsGeolocationAvailable && locator.IsGeolocationEnabled)
                    {
                        CancellationToken ct = new CancellationToken();
                        try
                        {
                            locator.DesiredAccuracy = 500;
                            var position = await locator.GetPositionAsync(TimeSpan.FromSeconds(10), ct);
                            if (position != null)
                            {
                                AddressLatitude = position.Latitude;
                                AddressLongitude = position.Longitude;

                                var res = await server.Activities.AddAddressPoint(AddressName.Trim(), position.Latitude, position.Longitude);
                                if (res.response == StatusCodes.OK)
                                {
                                    UserDialogs.Instance.Alert("Location added successfully");
                                    navigation.GoBack();
                                }
                                else
                                {
                                    UserDialogs.Instance.Alert("An error occurred while adding the address. Retry later");
                                    Debug.WriteLine("Error while adding address");
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            UserDialogs.Instance.Alert("An error occurred while retrieving location.\n" + e.Message);
                        }
                    }
                    else
                        UserDialogs.Instance.Alert("Your device doesn't have a location sensor or is disabled", "Location sensor not found");
                }
                else
                {
                    var location = await server.GoogleMaps.GetCoordinatesFromAddressAsync(AddressCity, AddressStreet, AddressCivicNumber, AddressZipCode);
                    if (location != null)
                    {
                        var response = await server.Activities.AddAddressPoint(AddressName, location.lat, location.lng);
                        if (response.response == StatusCodes.OK)
                        {
                            AddressLatitude = location.lat;
                            AddressLongitude = location.lng;
                            UserDialogs.Instance.Alert("Location added successfully");
                            navigation.GoBack();
                        }
                        else
                        {
                            UserDialogs.Instance.Alert("An error occurred while adding the address. Retry later");
                            Debug.WriteLine("Error while adding address");
                        }
                    }
                    else
                        UserDialogs.Instance.Alert("It was not possible retrieve coordinates");
                }
            }));
    }
}
