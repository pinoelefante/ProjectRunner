using Acr.UserDialogs;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
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
        private INavigationService navigation;
        public SearchActivityViewModel(PRServer s, PRCache c, INavigationService n)
        {
            server = s;
            cache = c;
            navigation = n;
        }
        public ObservableCollection<Sports> SportsAvailable { get; } = new ObservableCollection<Sports>()
        {
            Sports.RUNNING, Sports.FOOTBALL, Sports.BICYCLE, Sports.TENNIS
        };
        private int _selectedSportIndex;
        public int SelectedSportIndex { get { return _selectedSportIndex; } set { Set(ref _selectedSportIndex, value); } }
        private RelayCommand _searchCmd;
        public RelayCommand SearchCommand =>
            _searchCmd ??
            (_searchCmd = new RelayCommand(async () =>
            {
                Sports sport = SportsAvailable[SelectedSportIndex];
                var res = await server.Activities.SearchActivities(sport);
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
