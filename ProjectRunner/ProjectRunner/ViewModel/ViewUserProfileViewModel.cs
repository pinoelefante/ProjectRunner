using Acr.UserDialogs;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using ProjectRunner.ServerAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ProjectRunner.ViewModel
{
    public class ViewUserProfileViewModel : MyViewModel
    {
        private PRServer server;
        private PRCache cache;
        public ViewUserProfileViewModel(PRServer s, PRCache c, INavigationService n) : base(n)
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
            Task.Factory.StartNew(async () =>
            {
                IsCurrentUser = parameter == null || ((int)parameter) == cache.CurrentUser.Id;
                if (parameter != null)
                {
                    var response = await server.People.GetProfileInfo((int)parameter);
                    if (response.response == StatusCodes.OK)
                    {
                        User = response.content;
                        RaisePropertyChanged(() => User);
                        RaisePropertyChanged(() => IsCurrentUser);
                    }
                }
            });
        }
        private RelayCommand _addFriendCmd, _remFriend, _remRequest, _acceptReqCmd, _logoutCmd;
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
        public RelayCommand LogoutCommand =>
            _logoutCmd ??
            (_logoutCmd = new RelayCommand(async () =>
            {
                var res = await server.Authentication.Logout();
                if (res.response == StatusCodes.OK)
                    Application.Current.MainPage = new NavigationPage(new Views.LoginPage());
            }));
        private RelayCommand _takePhotoCmd;
        public RelayCommand TakePhotoCommand =>
            _takePhotoCmd ??
            (_takePhotoCmd = new RelayCommand(async () =>
            {
                if (!IsCurrentUser)
                    return;

                Debug.WriteLine("Opening photo");

                var cameraStatus = await CheckPermissionAsync(Permission.Camera);
                var storageStatus = await CheckPermissionAsync(Permission.Storage);
                
                if (cameraStatus && storageStatus)
                {
                    await CrossMedia.Current.Initialize();

                    if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                    {
                        UserDialogs.Instance.Alert("No Camera", ":( No camera available.", "OK");
                        return;
                    }

                    var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                    {
                        AllowCropping = true,
                        DefaultCamera = CameraDevice.Rear,
                        PhotoSize = PhotoSize.Small,
                        SaveToAlbum = false
                    });
                    
                    if (file != null)
                    {
                        byte[] content;
                        using (var stream = new BinaryReader(file.GetStream()))
                        {
                            content = stream.ReadBytes((int)stream.BaseStream.Length);
                        }
                        if (content!=null)
                        {
                            var checksum = MD5Core.GetHashString(content);
                            var ext = file.Path.Substring(file.Path.LastIndexOf("."));
                            var url = await server.User.UpdateProfileImage(content, ext, checksum);
                            if(url!=null)
                            {
                                User.Image = url;
                                RaisePropertyChanged(() => User);
                                RaisePropertyChanged(() => User.Image);
                                cache.SetUserImage(url);
                            }
                            else
                                UserDialogs.Instance.Alert("Image not updated");
                        }
                        else
                            UserDialogs.Instance.Alert("Image empty");
                    }
                    else
                        await UserDialogs.Instance.AlertAsync("Image error");
                }
                else
                {
                    await UserDialogs.Instance.AlertAsync("Permissions Denied", "Unable to take photos.", "OK");
                }

            }));
    }
}
