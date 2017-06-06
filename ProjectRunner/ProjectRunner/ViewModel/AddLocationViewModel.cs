using Acr.UserDialogs;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Plugin.Geolocator;
using ProjectRunner.ServerAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.Maps;

namespace ProjectRunner.ViewModel
{
    public class AddLocationViewModel : MyViewModel
    {
        private INavigationService navigation;
        private PRServer server;
        public AddLocationViewModel(INavigationService nav, PRServer s)
        {
            navigation = nav;
            server = s;
        }
        private string _mpName, _mpStreet, _mpCity, _mpZipCode;
        private int _mpCivicNumber;
        private double _mpLatitude, _mpLongitude;
        public string AddressName { get { return _mpName; } set { Set(ref _mpName, value); } }
        public string AddressStreet { get { return _mpStreet; } set { Set(ref _mpStreet, value); } }
        public string AddressCity { get { return _mpCity; } set { Set(ref _mpCity, value); } }
        public int AddressCivicNumber { get { return _mpCivicNumber; } set { Set(ref _mpCivicNumber, value); } }
        public string AddressZipCode { get { return _mpZipCode; } set { Set(ref _mpZipCode, value); } }
        public double AddressLatitude { get { return _mpLatitude; } set { Set(ref _mpLatitude, value); } }
        public double AddressLongitude { get { return _mpLongitude; } set { Set(ref _mpLongitude, value); } }
        public bool HasGPS { get { return CrossGeolocator.Current.IsGeolocationAvailable; } }

        private RelayCommand _findCoordinatesCmd, _addLocationCmd, _getMyPositionCmd;
        public RelayCommand FindCoordinatesCommand =>
            _findCoordinatesCmd ??
            (_findCoordinatesCmd = new RelayCommand(() =>
            {
                //TODO find coordinates Xamarin.Geolocation
                //var geo = new Geocoder();

            }));
        public RelayCommand AddLocationCommand =>
            _addLocationCmd ??
            (_addLocationCmd = new RelayCommand(async () =>
            {
                //TODO verify all fields aren't empty
                var res = await server.Activities.AddAddressPoint(AddressName, AddressLatitude, AddressLongitude);
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
            }));
        public RelayCommand GetMyGPSPositionCommand =>
            _getMyPositionCmd ??
            (_getMyPositionCmd = new RelayCommand(async () =>
            {
                var locator = CrossGeolocator.Current;
                if (locator.IsGeolocationAvailable && locator.IsGeolocationEnabled)
                {
                    CancellationToken ct = new CancellationToken();
                    try
                    {
                        var position = await locator.GetPositionAsync(TimeSpan.FromSeconds(5), ct);
                        if (position != null)
                        {
                            /*
                            var geo = new Geocoder();
                            var addrs = await geo.GetAddressesForPositionAsync(new Position(position.Latitude, position.Longitude));
                            var addr = addrs.FirstOrDefault();
                            if (addr != null)
                                UserDialogs.Instance.Alert(addr + $"\nTrovati {addrs.Count()} indirizzi");
                            */
                            AddressLatitude = position.Latitude;
                            AddressLongitude = position.Longitude;

                            /*
                            var addresses = await locator.GetAddressesForPositionAsync(position);
                            var address = addresses.FirstOrDefault();
                            if (address != null)
                            {
                                var country = address.CountryCode;
                                AddressZipCode = address.PostalCode;
                                AddressCity = address.Locality;
                                AddressStreet = address.Thoroughfare;
                            }
                            else
                            {
                                AddressCity = string.Empty;
                                AddressCivicNumber = 0;
                                AddressStreet = string.Empty;
                                AddressZipCode = string.Empty;
                                UserDialogs.Instance.Alert("Address not found", "");
                            }
                            */
                        }
                    }
                    catch (Exception e)
                    {
                        UserDialogs.Instance.Alert("An error occurred while retrieving location.\n" + e.Message);
                    }
                }
                else
                {
                    UserDialogs.Instance.Alert("Your device doesn't have a location sensor or is disabled", "Location sensor not found");
                }
            }));
    }
}
