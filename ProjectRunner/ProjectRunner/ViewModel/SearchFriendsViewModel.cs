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
    public class SearchFriendsViewModel : MyViewModel
    {
        private PRServer server;
        private INavigationService navigation;
        public SearchFriendsViewModel(PRServer s, INavigationService n)
        {
            server = s;
            navigation = n;
        }
        public ObservableCollection<UserProfile> SearchResults { get; } = new ObservableCollection<UserProfile>();
        private string oldSearchParameter = null;
        public string SearchParameter { get; set; } = string.Empty;
        private RelayCommand _searchCmd;
        public RelayCommand SearchCommand =>
            _searchCmd ??
            (_searchCmd = new RelayCommand(async () =>
            {
                
                if (!string.IsNullOrEmpty(SearchParameter.Trim()) && SearchParameter.Trim().Length >= 3 && !SearchParameter.Trim().Equals(oldSearchParameter))
                {
                    SearchResults.Clear();
                    //UserDialogs.Instance.ShowLoading("Searching for " + SearchParameter.Trim());
                    var res = await server.People.SearchPeople(SearchParameter.Trim());
                    if(res.response == StatusCodes.OK)
                    {
                        if(res.content!=null)
                        {
                            foreach (var item in res.content)
                                SearchResults.Add(item);
                        }
                        oldSearchParameter = SearchParameter.Trim();
                    }
                    //UserDialogs.Instance.HideLoading();
                    RaisePropertyChanged(() => SearchResults);
                }
            }));
        private RelayCommand<UserProfile> _openProfileCmd;
        public RelayCommand<UserProfile> OpenProfileCommand =>
            _openProfileCmd ??
            (_openProfileCmd = new RelayCommand<UserProfile>((x) =>
            {
                navigation.NavigateTo(ViewModelLocator.ViewUserProfile, x.Id);
            }));
    }
}
