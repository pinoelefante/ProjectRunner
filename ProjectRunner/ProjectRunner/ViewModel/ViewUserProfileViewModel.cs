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
                    User = response.content["user"] as UserProfile;
                    FriendsCount = (int)response.content["friendsCount"];
                    IsFriend = (bool)response.content["isFriend"];
                    IsFriendshipRequested = (bool)response.content["friendRequest"];
                    RaisePropertyChanged(() => User);
                    RaisePropertyChanged(() => FriendsCount);
                    RaisePropertyChanged(() => IsFriend);
                    RaisePropertyChanged(() => IsFriendshipRequested);
                }
            }
        }
    }
}
