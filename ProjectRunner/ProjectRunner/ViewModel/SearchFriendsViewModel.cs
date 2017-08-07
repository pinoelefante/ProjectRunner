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
        public SearchFriendsViewModel(PRServer s, INavigationService n) : base(n)
        {
            server = s;
        }
        public MyObservableCollection<UserProfile> SearchResults { get; } = new MyObservableCollection<UserProfile>();
        private string oldSearchParameter = null;
        public string SearchParameter { get; set; } = string.Empty;
        private RelayCommand _searchCmd;
        public RelayCommand SearchCommand =>
            _searchCmd ??
            (_searchCmd = new RelayCommand(async () =>
            {
                if (!string.IsNullOrEmpty(SearchParameter.Trim()) && SearchParameter.Trim().Length >= 3 && !SearchParameter.Trim().Equals(oldSearchParameter))
                {
                    var progress = new ProgressDialogConfig()
                    {
                        IsDeterministic = false,
                        Title = "Searching for "+SearchParameter.Trim(),
                        MaskType = MaskType.Gradient,
                        AutoShow = true,
                        CancelText = string.Empty,
                        OnCancel = null
                    };
                    using (UserDialogs.Instance.Progress(progress))
                    {
                        var res = await server.People.SearchPeople(SearchParameter.Trim());
                        if (res.response == StatusCodes.OK)
                        {
                            SearchResults.AddRange(res.content, true);
                            oldSearchParameter = SearchParameter.Trim();
                        }
                        RaisePropertyChanged(() => SearchResults);
                    } 
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
