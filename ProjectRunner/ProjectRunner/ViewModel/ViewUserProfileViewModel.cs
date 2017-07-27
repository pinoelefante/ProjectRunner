using GalaSoft.MvvmLight.Command;
using ProjectRunner.ServerAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectRunner.ViewModel
{
    public class ViewUserProfileViewModel : MyViewModel
    {
        private PRServer server;
        private PRCache cache;
        public ViewUserProfileViewModel(PRServer s, PRCache c)
        {
            server = s;
            cache = c;
        }
        public UserProfile User { get; set; }
        public bool IsCurrentUser { get; set; }
        public int FriendsCount { get; set; }
        public bool IsFriend { get; set; }
        public bool IsFriendshipRequested { get; set; }
        public override void NavigatedToAsync(object parameter = null)
        {
            IsCurrentUser = parameter == null || ((int)parameter) == cache.CurrentUser.Id;
            if(parameter != null)
            {
                var response = server.People.GetProfileInfo((int)parameter).Result;
                if (response.response == StatusCodes.OK)
                {
                    User = response.content;
                    RaisePropertyChanged(() => User);
                }
            }
        }
        private RelayCommand _addFriendCmd, _remFriend, _remRequest, _acceptReqCmd;
        public RelayCommand AddFriendCommand =>
            _addFriendCmd ??
            (_addFriendCmd = new RelayCommand(async () =>
            {
                var res = await server.People.RequestFriendship(User.Id);
                if(res.response == StatusCodes.OK)
                {
                    User.Status = FriendshipStatus.REQUESTED;
                    RaisePropertyChanged(() => User);
                }
            }));
        public RelayCommand RemoveFriendCommand =>
            _remFriend ??
            (_remFriend = new RelayCommand(async () =>
            {
                var res = await server.People.RemoveFriend(User.Id);
                if (res.response == StatusCodes.OK)
                {
                    User.Status = FriendshipStatus.STRANGER;
                    RaisePropertyChanged(() => User);
                }
            }));
        public RelayCommand RemoveFriendshipRequestCommand =>
            _remRequest ??
            (_remRequest = new RelayCommand(async () =>
            {
                var res = await server.People.RejectFriendship(User.Id);
                if (res.response == StatusCodes.OK)
                {
                    User.Status = FriendshipStatus.STRANGER;
                    RaisePropertyChanged(() => User);
                }
            }));
        public RelayCommand AcceptFriendshipRequest =>
            _acceptReqCmd ??
            (_acceptReqCmd = new RelayCommand(async () =>
            {
                var res = await server.People.AcceptFriendship(User.Id);
                if (res.response == StatusCodes.OK)
                {
                    User.Status = FriendshipStatus.IS_FRIEND;
                    RaisePropertyChanged(() => User);
                }
            }));
    }
}
