using Acr.UserDialogs;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using ProjectRunner.ServerAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ProjectRunner.ViewModel
{
    public class SearchActivityViewModel : MyViewModel
    {
        private PRServer server;
        private PRCache cache;
        public SearchActivityViewModel(PRServer s, PRCache c, INavigationService n) : base(n)
        {
            server = s;
            cache = c;
        }
        public override void NavigatedToAsync(object parameter = null)
        {
            base.NavigatedToAsync(parameter);

            LocationsList.Clear();
            foreach (var item in cache.MyMapAddresses)
                LocationsList.Add(item);
            LocationIndex = 0;
        }
        public ObservableCollection<Sports> SportsAvailable { get; } = new ObservableCollection<Sports>()
        {
            Sports.RUNNING, Sports.FOOTBALL, Sports.BICYCLE, Sports.TENNIS
        };
        public MapAddress SelectedAddress { get => LocationIndex >= 0 && LocationIndex < LocationsList.Count ? LocationsList[LocationIndex] : null; }
        
        public int LocationIndex { get => _indexLocation; set { Set(ref _indexLocation, value); RaisePropertyChanged(() => SelectedAddress); } }
        public ObservableCollection<MapAddress> LocationsList { get; } = new ObservableCollection<MapAddress>();
        private int _selectedSportIndex, _indexLocation;
        public int SelectedSportIndex { get => _selectedSportIndex; set => Set(ref _selectedSportIndex, value); }
        public List<int> Distances { get; } = new List<int>() { 2, 3, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50 };
        public int SelectedDistanceIndex { get; set; } = 2;
        private bool _useGps = false;
        public bool UseGPSLocation
        {
            get => _useGps;
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
                        if(!locator.IsGeolocationEnabled)
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
                RaisePropertyChanged(() => UseGPSLocation);
            }
        }
        private RelayCommand _searchCmd, _addLocationCmd;
        public RelayCommand SearchCommand =>
            _searchCmd ??
            (_searchCmd = new RelayCommand(async () =>
            {
                Sports sport = SportsAvailable[SelectedSportIndex];
                double latitude = 0, longitude = 0;
                
                if(UseGPSLocation)
                {
                    var locator = CrossGeolocator.Current;
                    try
                    {
                        locator.DesiredAccuracy = 500;
                        var location = await locator.GetPositionAsync(TimeSpan.FromSeconds(10));
                        latitude = location.Latitude;
                        longitude = location.Longitude;
                    }
                    catch
                    {
                        UserDialogs.Instance.Alert("There was an error while retrieving your gps position", "");
                        return;
                    }
                }
                else
                {
                    if(SelectedAddress==null)
                    {
                        UserDialogs.Instance.Alert("You have to select a starting address", "");
                        return;
                    }
                    latitude = SelectedAddress.Latitude;
                    longitude = SelectedAddress.Longitude;
                }
                
                var res = await server.Activities.SearchActivities(sport,latitude,longitude);
                if (res.response == StatusCodes.OK)
                {
                    SearchResults.Clear();
                    if (res.content != null)
                    {
                        SearchResults.AddRange(res.content);
                        RaisePropertyChanged(() => SearchResults);
                    }
                    if (SearchResults.Count > 0)
                        navigation.NavigateTo(ViewModelLocator.ActivitySearchResults);
                    else
                        UserDialogs.Instance.Alert($"I have found nothing", "Search results", "OK");
                }
                else
                {
                    UserDialogs.Instance.Alert("Search error", "Search error", "OK");
                }
            }));
        public List<Activity> SearchResults { get; } = new List<Activity>();
        private RelayCommand<Activity> _openActivityCmd;
        public RelayCommand<Activity> OpenActivityCommand =>
            _openActivityCmd ??
            (_openActivityCmd = new RelayCommand<Activity>((x) =>
            {
                navigation.NavigateTo(ViewModelLocator.ActivityDetails, x);
            }));
        public RelayCommand GoToAddLocation =>
            _addLocationCmd ??
            (_addLocationCmd = new RelayCommand(() =>
            {
                navigation.NavigateTo(ViewModelLocator.AddLocation);
            }));
    }
}
