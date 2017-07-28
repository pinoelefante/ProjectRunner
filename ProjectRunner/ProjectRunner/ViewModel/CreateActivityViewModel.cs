using Acr.UserDialogs;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Plugin.Geolocator;
using ProjectRunner.ServerAPI;
using ProjectRunner.Services;
using ProjectRunner.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace ProjectRunner.ViewModel
{
    public class CreateActivityViewModel : MyViewModel
    {
        private INavigationService navigation;
        private PRServer server;
        private PRCache cache;
        #region General
        public CreateActivityViewModel(INavigationService nav, PRServer s, PRCache c)
        {
            navigation = nav;
            server = s;
            cache = c;
        }
        public override void NavigatedToAsync(object parameter = null)
        {
            base.NavigatedToAsync(parameter);
            VerifyGeneral();
            VerifyLocation();
            RaisePropertyChanged(() => KnownAddress);
            RaisePropertyChanged(() => SelectedIndexLocation);
        }
        public override void NavigatedFrom()
        {
            base.NavigatedFrom();
        }
        private void Reset()
        {
            SelectedSportIndex = 0;
            SelectedIndexDistanceBicycle = 0;
            SelectedIndexDistanceRunning = 0;
            SelectedIndexGuestList = 0;
            SelectedIndexPlayerPerTeam = 0;
            SelectedIndexLocation = -1;
            Fee = 0f;
            Guests = 0;
            MaxPlayers = 1;
            WithFitness = false;
            IsDouble = false;
        }
        #endregion

        #region SportSelection
        private float _fee = 0;
        private int _guests, _maxPlayers, _requiredFeedback, _playersTeam;
        private int _indexSport, _indexPlayerPerTeam, _indexDistanceRunning, _indexDistanceBicycle, _indexGuestList, _indexCurrency;
        private bool _fitness, _isDouble, _isGratis = true;
        private DateTime _startDay = DateTime.Now;
        private TimeSpan _startTime = DateTime.Now.TimeOfDay.Add(TimeSpan.FromHours(1));

        public float Fee { get { return _fee; } set { Set(ref _fee, value); VerifyGeneral(); } }
        public ObservableCollection<string> CurrenciesList { get; } = new ObservableCollection<string>()
        {
            "EUR", "USD", "GBP", "CNY", "JPY"
        };
        //TODO select preferred currency
        public int SelectedCurrencyIndex { get { return _indexCurrency; } set { Set(ref _indexCurrency, value); RaisePropertyChanged(() => SelectedCurrency); } }
        public string SelectedCurrency { get { if (SelectedCurrencyIndex < 0) return null; return CurrenciesList[SelectedCurrencyIndex]; } }
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
                RaisePropertyChanged(() => Distance);
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
                RaisePropertyChanged(() => Distance);
            }
        }
        public int Distance
        {
            get
            {
                switch(SelectedSport?.SportEnumValue)
                {
                    case Sports.BICYCLE:
                        return BicycleDistances[SelectedIndexDistanceBicycle];
                    case Sports.RUNNING:
                        return RunningDistances[SelectedIndexDistanceRunning];
                }
                return 0;
            }
        }

        public int PlayersPerTeam { get { return _playersTeam; } set { Set(ref _playersTeam, value); CalculateMaxPlayers(); VerifyGeneral(); } }
        public int Guests { get { return _guests; } set { Set(ref _guests, value); CalculateMaxPlayers(); VerifyGeneral(); } }
        public bool IsDouble { get { return _isDouble; } set { Set(ref _isDouble, value); CalculateMaxPlayers(); VerifyGeneral(); } }
        private void CalculateMaxPlayers()
        {
            switch (SelectedSport?.SportEnumValue)
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

                    UserDialogs.Instance.Alert("Max players can't be lesser than 1 + Guests");
                    Set(ref _maxPlayers, 1 + Guests);
                }
                VerifyGeneral();
            }
        }
        public int RequiredFeedback { get { return _requiredFeedback; } set { Set(ref _requiredFeedback, value); VerifyGeneral(); } }
        public bool WithFitness { get { return _fitness; } set { Set(ref _fitness, value); VerifyGeneral(); } }
        public DateTime StartDay { get { return _startDay; } set { Set(ref _startDay, value); VerifyGeneral(); } }
        public TimeSpan StartTime { get { return _startTime; } set { Set(ref _startTime, value); VerifyGeneral(); RaisePropertyChanged(() => StartTimeString); } }
        public string StartTimeString { get { return $"{StartTime.Hours.ToString("D2")}:{StartTime.Minutes.ToString("D2")}"; } }
        public bool IsGratis { get { return _isGratis; } set { Set(ref _isGratis, value); Fee = value ? 0.0f : 1.0f; VerifyGeneral(); } }
        public int SelectedSportIndex
        {
            get { return _indexSport; }
            set
            {
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
                switch (SelectedSport?.SportEnumValue)
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
                switch (SelectedSport?.SportEnumValue)
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
                if (StartDay.Date.CompareTo(DateTime.Now.Date) < 0)
                {
                    IsNextGeneralEnabled = false;
                    return;
                }
                var timeStart = StartDay.Date.AddHours(StartTime.Hours).AddMinutes(StartTime.Minutes);
                if(timeStart.CompareTo(DateTime.Now)<=0)
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
                switch (SelectedSport?.SportEnumValue)
                {
                    case Sports.BICYCLE:
                        IsNextGeneralEnabled = SelectedIndexDistanceBicycle >= 0;
                        return;
                    case Sports.RUNNING:
                        IsNextGeneralEnabled = SelectedIndexDistanceRunning >= 0;
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
            else
                IsNextGeneralEnabled = false;
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
        public List<MapAddress> KnownAddress { get { return cache.MyMapAddresses.OrderBy(x => x.Name).ToList(); } }
        
        private int _selectedIndexLocation = 0;
        public int SelectedIndexLocation { get { return _selectedIndexLocation; } set { Set(ref _selectedIndexLocation, value); VerifyLocation(); RaisePropertyChanged(()=> SelectedLocation); } }
        public MapAddress SelectedLocation { get { if (_selectedIndexLocation >= 0 && KnownAddress.Count() > _selectedIndexLocation) return KnownAddress[_selectedIndexLocation]; return null; } }
        private RelayCommand _deleteLocationCmd;
        public RelayCommand DeleteLocationCommand =>
            _deleteLocationCmd ??
            (_deleteLocationCmd = new RelayCommand(async () =>
            {
                var address = KnownAddress[SelectedIndexLocation];
                var res = await server.Activities.RemoveAddress(address.Id);
                if(res.response == StatusCodes.OK)
                {
                    RaisePropertyChanged(() => KnownAddress);
                    RaisePropertyChanged(() => SelectedLocation);
                    address = null;
                }
            }));
        public RelayCommand GoToConfirm =>
            _goToConfirmCmd ??
            (_goToConfirmCmd = new RelayCommand(() =>
            {
                navigation.NavigateTo(ViewModelLocator.CreateActivityConfirm);
            }));
        
        private bool _nextLocationEnabled = false;
        public bool IsNextLocationEnabled { get { return _nextLocationEnabled; } set { Set(ref _nextLocationEnabled, value); } }
        private void VerifyLocation()
        {
            IsNextLocationEnabled = SelectedIndexLocation >= 0 && SelectedIndexLocation < KnownAddress.Count;
        }
        #endregion

        #region Add Location
        private RelayCommand _goAddLocationCmd;
        public RelayCommand GoToAddLocation =>
            _goAddLocationCmd ??
            (_goAddLocationCmd = new RelayCommand(()=> 
            {
                navigation.NavigateTo(ViewModelLocator.AddLocation);
            }));
        

        #endregion

        #region Confirm activity
        private RelayCommand _createActivityCmd;
        public RelayCommand CreateActivityCommand =>
            _createActivityCmd ?? 
            (_createActivityCmd = new RelayCommand(
            async () =>
            {

                Dictionary<string, string> sportDetails = null;
                switch (SelectedSport?.SportEnumValue)
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

                var response = await server.Activities.CreateActivityAsync(StartDay, StartTime, SelectedLocation.Id, MaxPlayers, Guests, Fee, SelectedCurrency, SelectedSport.SportEnumValue, RequiredFeedback, sportDetails);
                if (response.response == StatusCodes.OK)
                {
                    UserDialogs.Instance.Alert("Activity created", "Activity creation");
                    navigation.NavigateTo(ViewModelLocator.Activities);
                    Reset();
                }
                else
                {
                    UserDialogs.Instance.Alert("An error occurred while creating the activity", "Activity creation");
                }
            }));
        #endregion
        private bool _nextGeneralEnabled = false;
        public bool IsNextGeneralEnabled { get { return _nextGeneralEnabled; } set { Set(ref _nextGeneralEnabled, value); } }
    }
}
