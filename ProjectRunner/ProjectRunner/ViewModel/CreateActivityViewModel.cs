using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using ProjectRunner.ServerAPI;
using ProjectRunner.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectRunner.ViewModel
{
    public class CreateActivityViewModel : MyViewModel
    {
        private INavigationService navigation;
        private PRServer server;
        private UserDialogsService dialogs;
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
        }
        public override void NavigatedFrom()
        {
            base.NavigatedFrom();
            Reset(true);
        }
        private void Reset(bool sport = false)
        {
            if(sport)
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
            MeetingAddress = string.Empty;
            MeetingLatitude = null;
            MeetingLongitude = null;
        }
        private float? _mpLong, _mpLat;
        private string _mpAddr;
        private float _fee = 0, _distance;
        private int _guests, _maxPlayers, _requiredFeedback, _playersTeam;
        private int _indexSport, _indexPlayerPerTeam, _indexDistanceRunning, _indexDistanceBicycle, _indexGuestList;
        private bool _fitness, _isDouble, _isGratis, _hasGps;
        private DateTime _startDay = DateTime.Now;
        private TimeSpan _startTime = DateTime.Now.TimeOfDay;

        public float? MeetingLatitude { get { return _mpLat; } set { Set(ref _mpLat, value); } }
        public float? MeetingLongitude { get { return _mpLong; } set { Set(ref _mpLong, value); } }
        public string MeetingAddress { get { return _mpAddr; } set { Set(ref _mpAddr, value); } }
        public float Fee { get { return _fee; } set { Set(ref _fee, value); } }
        public float Distance { get { return _distance; } set { Set(ref _distance, value); } }
        public int Guests { get { return _guests; } set { Set(ref _guests, value); CalculateMaxPlayers(); } }

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
            }
        }
        public int RequiredFeedback { get { return _requiredFeedback; } set { Set(ref _requiredFeedback, value); } }
        public bool WithFitness { get { return _fitness; } set { Set(ref _fitness, value); } }
        public bool IsDouble { get { return _isDouble; } set { Set(ref _isDouble, value); CalculateMaxPlayers(); } }
        public DateTime StartDay { get { return _startDay; } set { Set(ref _startDay, value); } }
        public TimeSpan StartTime { get { return _startTime; } set { Set(ref _startTime,value); } }
        public bool IsGratis
        {
            get { return _isGratis; }
            set
            {
                Set(ref _isGratis, value);
                Fee = value ? 0.0f : 1.0f;
            }
        }
        public int SelectedSportIndex
        {
            get { return _indexSport; }
            set
            {
                Reset();
                Set(ref _indexSport, value);
                if(value >= 0)
                    SelectedSport = SportsAvailable[value].SportEnumValue;
                
                RaisePropertyChanged(() => SelectedSport);
                RaisePropertyChanged(() => IsMaxPlayerActive);
                RaisePropertyChanged(() => IsGuestListActive);
            }
        }
        private Sports _selectedSport;
        public Sports SelectedSport { get { return _selectedSport; } set { Set(ref _selectedSport, value); } }
        public int SelectedIndexPlayerPerTeam
        {
            get { return _indexPlayerPerTeam; }
            set
            {
                Set(ref _indexPlayerPerTeam, value);
                if(value >= 0)
                    PlayersPerTeam = FootballPlayersPerTeam[value];
            }
        }
        public int PlayersPerTeam
        {
            get
            {
                return _playersTeam;
            }
            set
            {
                Set(ref _playersTeam, value);
                CalculateMaxPlayers();
            }
        }
        private List<MapAddress> _addresses;
        public List<MapAddress> KnownAddress { get { return _addresses; } set { Set(ref _addresses, value); } }
        public ObservableCollection<SportItem> SportsAvailable { get; } = new ObservableCollection<SportItem>()
        {
            new SportItem(Sports.RUNNING, "Corsa"),
            new SportItem(Sports.FOOTBALL, "Calcio"),
            new SportItem(Sports.BICYCLE, "Bicicletta"),
            new SportItem(Sports.TENNIS, "Tennis")
        };
        public ObservableCollection<int> RunningDistances { get; } = new ObservableCollection<int>()
        {
            3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,42
        };
        public ObservableCollection<int> BicycleDistances { get; } = new ObservableCollection<int>()
        {
            5,10,15,20,25,30,35,40,45,50,60,70,80,90,100
        };
        public ObservableCollection<int> FootballPlayersPerTeam { get; } = new ObservableCollection<int>()
        {
            5,6,7,8,9,10,11
        };
        public bool HasGPS { get { return _hasGps; } set { Set(ref _hasGps, value); } }
        
        private void CalculateMaxPlayers()
        {
            switch (SelectedSport)
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
        public bool IsMaxPlayerActive
        {
            get
            {
                switch(SelectedSport)
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
                switch (SelectedSport)
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
        public int SelectedIndexDistanceRunning
        {
            get { return _indexDistanceRunning; }
            set
            {
                Set(ref _indexDistanceRunning, value);
                if(value >= 0)
                    Distance = RunningDistances[value];
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
        public RelayCommand CreateActivityCommand => new RelayCommand(
            async () =>
            {

                Dictionary<string, string> sportDetails = null;
                switch (SelectedSport)
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
                //TODO Remove 1 from default meeting point value
                var response = await server.Activities.CreateActivityAsync(StartDay, StartTime, 1, MaxPlayers, Guests, Fee, SelectedSport, RequiredFeedback, sportDetails);
                if(response.response == StatusCodes.OK)
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
