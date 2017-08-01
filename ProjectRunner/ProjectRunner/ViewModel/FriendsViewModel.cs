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
    public class FriendsViewModel : MyViewModel
    {
        private PRServer server;
        public FriendsViewModel (INavigationService n, PRServer s) : base(n)
        {
            server = s;
        }
        public override void NavigatedToAsync(object parameter = null)
        {
            LoadFriendsAsync();
        }
        private List<UserProfile> FriendsList { get; set; }
        public List<UserProfile> Friends { get { return FriendsList!=null ? FriendsList.Where(x => x.Status == FriendshipStatus.IS_FRIEND).ToList() : null; } }
        public List<UserProfile> RequestsSent { get { return FriendsList != null ? FriendsList.Where(x => x.Status == FriendshipStatus.REQUESTED).ToList() : null; } }
        public List<UserProfile> RequestsReceived { get { return FriendsList != null ? FriendsList.Where(x => x.Status == FriendshipStatus.RECEIVED).ToList() : null; } }
        private async Task LoadFriendsAsync()
        {
            var response = await server.People.FriendList();
            if(response.response == StatusCodes.OK)
            {
                FriendsList = response.content;
                RaisePropertyChanged(() => Friends);
                RaisePropertyChanged(() => RequestsSent);
                RaisePropertyChanged(() => RequestsReceived);
            }
            else
                UserDialogs.Instance.Alert("Check your internet connection", "Error: "+response.response);
        }
        private RelayCommand<UserProfile> _openProfile;
        public RelayCommand<UserProfile> OpenProfileCommand =>
            _openProfile ??
            (_openProfile = new RelayCommand<UserProfile>((x) =>
            {
                navigation.NavigateTo(ViewModelLocator.ViewUserProfile, x.Id);
            }));
        private RelayCommand _searchFriend;
        public RelayCommand SearchFriendCommand =>
            _searchFriend ??
            (_searchFriend = new RelayCommand(() =>
            {
                navigation.NavigateTo(ViewModelLocator.SearchFriendsPage);
            }));
    }
}
