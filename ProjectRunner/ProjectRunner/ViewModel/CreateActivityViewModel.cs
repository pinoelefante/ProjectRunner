﻿using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Plugin.Geolocator;
using ProjectRunner.ServerAPI;
using ProjectRunner.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectRunner.ViewModel
{
    public class CreateActivityViewModel : MyViewModel
    {
        private INavigationService navigation;
        private PRServer server;
        private UserDialogsService dialogs;
        #region General
        public CreateActivityViewModel(INavigationService nav, PRServer s, UserDialogsService u)
        {
            navigation = nav;
            server = s;
            dialogs = u;
        }
        public override void NavigatedTo(object parameter = null)
        {
            base.NavigatedTo(parameter);
            //rilevare gps
            //load my locations
            LoadMyAddressesAsync();
        }
        public override void NavigatedFrom()
        {
            base.NavigatedFrom();
            //Reset(true);
        }
        
        public override bool OnBackPressed()
        {
            return true;
        }
        private void Reset(bool sport = false)
        {
            if (sport)
                SelectedSportIndex = 0;
            SelectedIndexDistanceBicycle = 0;
            SelectedIndexDistanceRunning = 0;
            SelectedIndexGuestList = 0;
            SelectedIndexPlayerPerTeam = 0;
            Fee = 0f;
            Guests = 0;
            MaxPlayers = 1;
            WithFitness = false;
            IsDouble = false;
        }
        #endregion

        #region SportSelection
        private float _fee = 0, _distance = 0;
        private int _guests, _maxPlayers, _requiredFeedback, _playersTeam;
        private int _indexSport, _indexPlayerPerTeam, _indexDistanceRunning, _indexDistanceBicycle, _indexGuestList;
        private bool _fitness, _isDouble, _isGratis = true, _hasGps;
        private DateTime _startDay = DateTime.Now;
        private TimeSpan _startTime = DateTime.Now.TimeOfDay;

        public float Fee { get { return _fee; } set { Set(ref _fee, value); VerifyGeneral(); } }

        public float Distance { get { return _distance; } set { Set(ref _distance, value); VerifyGeneral(); } }
        public ObservableCollection<int> RunningDistances { get; } = new ObservableCollection<int>()
        {
            3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,42
        };
        public int SelectedIndexDistanceRunning
        {
            get { return _indexDistanceRunning; }
            set
            {
                Set(ref _indexDistanceRunning, value);
                if (value >= 0)
                    Distance = RunningDistances[value];
            }
        }
        public ObservableCollection<int> BicycleDistances { get; } = new ObservableCollection<int>()
        {
            5,10,15,20,25,30,35,40,45,50,60,70,80,90,100
        };
        public int SelectedIndexDistanceBicycle
        {
            get { return _indexDistanceBicycle; }
            set
            {
                Set(ref _indexDistanceBicycle, value);
                if (value >= 0)
                    Distance = BicycleDistances[value];
            }
        }

        public int PlayersPerTeam { get { return _playersTeam; } set { Set(ref _playersTeam, value); CalculateMaxPlayers(); VerifyGeneral(); } }
        public int Guests { get { return _guests; } set { Set(ref _guests, value); CalculateMaxPlayers(); VerifyGeneral(); } }
        public bool IsDouble { get { return _isDouble; } set { Set(ref _isDouble, value); CalculateMaxPlayers(); VerifyGeneral(); } }
        private void CalculateMaxPlayers()
        {
            switch (SelectedSport.SportEnumValue)
            {
                case Sports.RUNNING:
                case Sports.BICYCLE:
                    MaxPlayers = 1 + Guests;
                    break;
                case Sports.FOOTBALL:
                    MaxPlayers = (2 * PlayersPerTeam);
                    break;
                case Sports.TENNIS:
                    MaxPlayers = IsDouble ? 4 : 2;
                    break;
            }
        }

        public int MaxPlayers
        {
            get
            {
                return _maxPlayers;
            }
            set
            {
                if (value >= 1 + Guests)
                    Set(ref _maxPlayers, value);
                else
                {

                    dialogs.ShowAlert("Max players can't be lesser than 1 + Guests");
                    Set(ref _maxPlayers, 1 + Guests);
                }
                VerifyGeneral();
            }
        }
        public int RequiredFeedback { get { return _requiredFeedback; } set { Set(ref _requiredFeedback, value); VerifyGeneral(); } }
        public bool WithFitness { get { return _fitness; } set { Set(ref _fitness, value); VerifyGeneral(); } }
        public DateTime StartDay { get { return _startDay; } set { Set(ref _startDay, value); VerifyGeneral(); RaisePropertyChanged(() => StartDayString); } }
        public string StartDayString { get { return StartDay.ToString("d"); } }
        public TimeSpan StartTime { get { return _startTime; } set { Set(ref _startTime, value); VerifyGeneral(); RaisePropertyChanged(() => StartTimeString); } }
        public string StartTimeString { get { return $"{StartTime.Hours.ToString("D2")}:{StartTime.Minutes.ToString("D2")}"; } }
        public bool IsGratis { get { return _isGratis; } set { Set(ref _isGratis, value); Fee = value ? 0.0f : 1.0f; VerifyGeneral(); } }
        public int SelectedSportIndex
        {
            get { return _indexSport; }
            set
            {
                Reset();
                Set(ref _indexSport, value);

                RaisePropertyChanged(() => SelectedSport);
                RaisePropertyChanged(() => IsMaxPlayerActive);
                RaisePropertyChanged(() => IsGuestListActive);
            }
        }

        public SportItem SelectedSport { get { if (_indexSport >= 0) return SportsAvailable[_indexSport]; return null; } }
        public ObservableCollection<SportItem> SportsAvailable { get; } = new ObservableCollection<SportItem>()
        {
            new SportItem(Sports.RUNNING, "Corsa"),
            new SportItem(Sports.FOOTBALL, "Calcio"),
            new SportItem(Sports.BICYCLE, "Bicicletta"),
            new SportItem(Sports.TENNIS, "Tennis")
        };
        public ObservableCollection<int> FootballPlayersPerTeam { get; } = new ObservableCollection<int>()
        {
            5,6,7,8,9,10,11
        };
        public int SelectedIndexPlayerPerTeam
        {
            get { return _indexPlayerPerTeam; }
            set
            {
                Set(ref _indexPlayerPerTeam, value);
                if (value >= 0)
                    PlayersPerTeam = FootballPlayersPerTeam[value];
            }
        }
        public bool IsMaxPlayerActive
        {
            get
            {
                switch (SelectedSport.SportEnumValue)
                {
                    case Sports.RUNNING:
                    case Sports.BICYCLE:
                        return true;
                    case Sports.TENNIS:
                    case Sports.FOOTBALL:
                        return false;
                }
                return true;
            }
        }
        public bool IsGuestListActive
        {
            get
            {
                switch (SelectedSport.SportEnumValue)
                {
                    case Sports.BICYCLE:
                        return false;
                    case Sports.FOOTBALL:
                        return true;
                    case Sports.RUNNING:
                        return false;
                    case Sports.TENNIS:
                        return true;
                }
                return true;
            }
        }

        public int SelectedIndexGuestList
        {
            get { return _indexGuestList; }
            set
            {
                Set(ref _indexGuestList, value);
                Guests = value;
            }
        }
        private void VerifyGeneral()
        {
            if (SelectedSportIndex >= 0)
            {
                if (StartDay.CompareTo(DateTime.Now) < 0)
                {
                    IsNextGeneralEnabled = false;
                    return;
                }
                if (IsGuestListActive && SelectedIndexGuestList < 0)
                {
                    IsNextGeneralEnabled = false;
                    return;
                }
                if (MaxPlayers == 0 || MaxPlayers < 1 + Guests)
                {
                    IsNextGeneralEnabled = false;
                    return;
                }
                if (!IsGratis && Fee < 1)
                {
                    IsNextGeneralEnabled = false;
                    return;
                }
            }
            switch (SelectedSport.SportEnumValue)
            {
                case Sports.BICYCLE:
                case Sports.RUNNING:
                    IsNextGeneralEnabled = Distance > 0;
                    return;
                case Sports.FOOTBALL:
                    IsNextGeneralEnabled = PlayersPerTeam >= 5;
                    return;
                case Sports.TENNIS:
                    IsNextGeneralEnabled = true;
                    return;
                default:
                    IsNextGeneralEnabled = false;
                    return;
            }
        }

        private RelayCommand _goLocationCmd, _goToConfirmCmd;
        public RelayCommand GoToLocation =>
            _goLocationCmd ??
            (_goLocationCmd = new RelayCommand(() =>
            {
                navigation.NavigateTo(ViewModelLocator.CreateActivityChooseLocation);
            }));



        #endregion

        #region Choose location
        public bool HasGPS { get { return _hasGps; } set { Set(ref _hasGps, value); } }
        public ObservableCollection<MapAddress> KnownAddress { get; } = new ObservableCollection<MapAddress>();
        private async Task LoadMyAddressesAsync()
        {
            var addr = await server.Activities.ListAddressAsync();
            if (addr.response == StatusCodes.OK)
            {
                KnownAddress.Clear();
                foreach (var item in addr.content)
                    KnownAddress.Add(item);
                RaisePropertyChanged(() => KnownAddress);
            }

            foreach (var item in KnownAddress)
            {
                Debug.WriteLine(item.Name);
            }
        }
        private int _selectedIndexLocation;
        public int SelectedIndexLocation { get { return _selectedIndexLocation; } set { Set(ref _selectedIndexLocation, value); RaisePropertyChanged(()=> SelectedLocation); } }
        public MapAddress SelectedLocation { get { if (_selectedIndexLocation >= 0 && KnownAddress.Count() > _selectedIndexLocation) return KnownAddress[_selectedIndexLocation]; return null; } }
        public RelayCommand GoToConfirm =>
            _goToConfirmCmd ??
            (_goToConfirmCmd = new RelayCommand(() =>
            {
                navigation.NavigateTo(ViewModelLocator.CreateActivityConfirm);
            }));
        public RelayCommand TestLocation =>
            new RelayCommand(() =>
            {
                if (SelectedLocation != null)
                    Debug.WriteLine(SelectedLocation);
                else
                    Debug.WriteLine("Location non selezionata");
            });
        #endregion

        #region Add Location
        private RelayCommand _goAddLocationCmd;
        public RelayCommand GoToAddLocation =>
            _goAddLocationCmd ??
            (_goAddLocationCmd = new RelayCommand(()=> 
            {
                navigation.NavigateTo(ViewModelLocator.CreateActivityAddLocation);
            }));
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

        private RelayCommand _findCoordinatesCmd, _addLocationCmd, _getMyPositionCmd;
        public RelayCommand FindCoordinatesCommand =>
            _findCoordinatesCmd ??
            (_findCoordinatesCmd = new RelayCommand(() =>
            {
                //TODO find coordinates Xamarin.Geolocation
                
            }));
        public RelayCommand AddLocationCommand =>
            _addLocationCmd ??
            (_addLocationCmd = new RelayCommand(async () =>
            {
                //TODO verify all fields aren't empty
                var res = await server.Activities.AddAddressPoint(AddressName, AddressLatitude, AddressLongitude);
                if(res.response == StatusCodes.OK)
                {
                    await LoadMyAddressesAsync();
                    navigation.GoBack();
                }
                else
                {
                    dialogs.ShowAlert("An error occurred while adding the address. Retry later");
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
                            AddressLatitude = position.Latitude;
                            AddressLongitude = position.Longitude;

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
                                dialogs.ShowAlert("Address not found", "");
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        dialogs.ShowAlert("An error occurred while retrieving location");
                    }
                }
                else
                {
                    dialogs.ShowAlert("Your device doesn't have a location sensor or is disabled", "Location sensor not found");
                }
            }));
        #endregion

        #region Confirm activity

        public RelayCommand CreateActivityCommand => new RelayCommand(
            async () =>
            {

                Dictionary<string, string> sportDetails = null;
                switch (SelectedSport.SportEnumValue)
                {
                    case Sports.BICYCLE:
                        sportDetails = BicycleActivity.CreateDetailsDictionary(Distance);
                        break;
                    case Sports.FOOTBALL:
                        sportDetails = FootballActivity.CreateDetailsDictionary(PlayersPerTeam);
                        break;
                    case Sports.RUNNING:
                        sportDetails = RunningActivity.CreateDetailsDictionary(Distance, null, WithFitness);
                        break;
                    case Sports.TENNIS:
                        sportDetails = TennisActivity.CreateDetailsDictionary(IsDouble);
                        break;
                }

                Debug.WriteLine(StartDay.ToString());
                Debug.WriteLine(StartTime.ToString());

                var response = await server.Activities.CreateActivityAsync(StartDay, StartTime, SelectedLocation.Id, MaxPlayers, Guests, Fee, SelectedSport.SportEnumValue, RequiredFeedback, sportDetails);
                if (response.response == StatusCodes.OK)
                {
                    dialogs.ShowAlert("Activity created", "Activity creation");
                    navigation.NavigateTo(ViewModelLocator.HomePage);
                }
                else
                {
                    dialogs.ShowAlert("An error occurred while creating the activity", "Activity creation");
                }
            }
            );
        #endregion
        private bool _nextGeneralEnabled = false;
        public bool IsNextGeneralEnabled { get { return _nextGeneralEnabled; } set { Set(ref _nextGeneralEnabled, value); } }
        
        public class SportItem
        {
            public Sports SportEnumValue { get; }
            public string SportName { get; }
            public SportItem(Sports e, string n)
            {
                SportEnumValue = e;
                SportName = n;
            }
        }
    }
}
