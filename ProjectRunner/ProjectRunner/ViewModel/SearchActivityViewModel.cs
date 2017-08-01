using Acr.UserDialogs;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Plugin.Geolocator;
using ProjectRunner.ServerAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public ObservableCollection<Sports> SportsAvailable { get; } = new ObservableCollection<Sports>()
        {
            Sports.RUNNING, Sports.FOOTBALL, Sports.BICYCLE, Sports.TENNIS
        };
        private int _selectedSportIndex;
        public int SelectedSportIndex { get { return _selectedSportIndex; } set { Set(ref _selectedSportIndex, value); } }
        public List<int> Distances { get; } = new List<int>() { 2, 3, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50 };
        public int SelectedDistanceIndex { get; set; } = 2;
        public bool UseGPSLocation { get; set; } = false;
        private RelayCommand _searchCmd;
        public RelayCommand SearchCommand =>
            _searchCmd ??
            (_searchCmd = new RelayCommand(async () =>
            {
                Sports sport = SportsAvailable[SelectedSportIndex];
                double latitude = 0, longitude = 0;
                bool haveDefaultAddress = false;
                if(cache.CurrentUser.DefaultUserLocation > 0)
                {
                    var address = cache.MyMapAddresses.Find(x => x.Id == cache.CurrentUser.DefaultUserLocation);
                    if(address != null)
                    {
                        haveDefaultAddress = true;
                        latitude = address.Latitude;
                        longitude = address.Longitude;
                    }
                }
                if(UseGPSLocation)
                {
                    var locator = CrossGeolocator.Current;
                    if (locator.IsGeolocationAvailable && locator.IsGeolocationEnabled)
                    {
                        try
                        {
                            var lastPosition = await locator.GetLastKnownLocationAsync();
                            if (lastPosition != null)
                            {
                                latitude = lastPosition.Latitude;
                                longitude = lastPosition.Longitude;
                            }
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
                        UserDialogs.Instance.Alert("Your device have position sensor disabled or is not available", "Position error");
                        return;
                    }
                }
                else if(!haveDefaultAddress)
                {
                    UserDialogs.Instance.Alert("You have to register one address as default address", "");
                    return;
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
    }
}
