using Acr.UserDialogs;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Plugin.ExternalMaps;
using ProjectRunner.ServerAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ProjectRunner.ViewModel
{
    public class ActivityDetailsViewModel : MyViewModel
    {
        private PRServer server;
        private INavigationService navigation;
        private PRCache cache;
        public ActivityDetailsViewModel(PRServer s, INavigationService n, PRCache c)
        {
            server = s;
            navigation = n;
            cache = c;
        }
        public Activity CurrentActivity { get; private set; }
        public override void NavigatedTo(object parameter = null)
        {
            if (parameter != null && parameter is Activity)
            {
                IsEditModeEnabled = false;
                CurrentActivity = parameter as Activity;
                IsMyActivity = cache.MyUserId == CurrentActivity.CreatedBy;
                RaisePropertyChanged(() => CurrentActivity);
                RaisePropertyChanged(() => IsMyActivity);
                RaisePropertyChanged(() => IsEditModeEnabled);
                RefreshPeopleListCommand.Execute(null);
            }
            else
                navigation.NavigateTo(ViewModelLocator.Activities);
        }
        public bool IsMyActivity { get; set; }
        public bool IsEditModeEnabled { get; set; }
        private RelayCommand _enableEditModeCmd, _saveChangesCmd, _openMapCmd, _leaveActivityCmd, _deleteActivityCmd, _sendChatMsgCmd;
        public RelayCommand ToogleEditModeCommand =>
            _enableEditModeCmd ??
            (_enableEditModeCmd = new RelayCommand(() =>
            {
                IsEditModeEnabled = !IsEditModeEnabled;
                RaisePropertyChanged(() => IsEditModeEnabled);
            }));
        public RelayCommand OpenMapCommand =>
            _openMapCmd ??
            (_openMapCmd = new RelayCommand(async () =>
            {
                try
                {
                    var res = await CrossExternalMaps.Current.NavigateTo("Rendez-vous point", CurrentActivity.MeetingPoint.Latitude, CurrentActivity.MeetingPoint.Longitude);
                    if (!res)
                        Debug.WriteLine("Map error!");
                }
                catch(Exception e)
                {
                    Debug.WriteLine(e.Message);
                    Device.OpenUri(new Uri($"https://www.google.com/maps/@{CurrentActivity.MeetingPoint.Latitude},{CurrentActivity.MeetingPoint.Longitude},17z"));   
                }
            }));
        public RelayCommand SaveChangesCommand =>
            _saveChangesCmd ??
            (_saveChangesCmd = new RelayCommand(() =>
            {
                //TODO
            }));
        public RelayCommand DeleteActivityCommand =>
            _deleteActivityCmd ??
            (_deleteActivityCmd = new RelayCommand(async () =>
            {
                CancellationToken cts = new CancellationToken();
                if (await UserDialogs.Instance.ConfirmAsync("Are you sure to delete this activity?", "Delete activity", "Yes", "No", cts))
                {
                    var res = await server.Activities.DeleteActivityAsync(CurrentActivity.Id);
                    if (res.response == StatusCodes.OK)
                    {
                        if (cache.ListActivities.Remove(CurrentActivity))
                            Debug.WriteLine("Item removed from cache");
                        navigation.GoBack();
                    }
                }

            }));
        public RelayCommand LeaveActivityCommand =>
            _leaveActivityCmd ??
            (_leaveActivityCmd = new RelayCommand(async () =>
            {
                if (CurrentActivity.CreatedBy == cache.MyUserId)
                {
                    var confirm = await UserDialogs.Instance.ConfirmAsync("If you leave, the activity will deleted. Do you want continue?", "Delete activity", "Yes", "No");
                    Debug.WriteLine("Confirm: " + confirm);
                    if(confirm)
                        DeleteActivityCommand.Execute(null);
                }
                else
                {
                    CancellationToken cts = new CancellationToken();
                    if (await UserDialogs.Instance.ConfirmAsync("Are you sure to leave this activity?", "Leave activity", "Yes", "No", cts))
                    {
                        var res = await server.Activities.LeaveActivityAsync(CurrentActivity.Id);
                        if (res.response == StatusCodes.OK)
                        {
                            if (cache.ListActivities.Remove(CurrentActivity))
                                Debug.WriteLine("Item removed from cache");
                            navigation.GoBack();
                        }
                    }
                }
            }));
        private string _chatMessage;
        public string ChatMessage { get { return _chatMessage; } set { Set(ref _chatMessage, value); } }
        public RelayCommand SendChatMessageCommand =>
            _sendChatMsgCmd ??
            (_sendChatMsgCmd = new RelayCommand(() =>
            {
                if(ChatMessage.Trim().Length > 0)
                {
                    //TODO send message
                    ChatMessage = string.Empty;
                }
            }));
        public ObservableCollection<UserProfile> ActivityPeople { get; } = new ObservableCollection<UserProfile>();
        public bool IsLoadingPeople { get; set; }
        private RelayCommand _refreshPeopleListCmd;
        public RelayCommand RefreshPeopleListCommand =>
            _refreshPeopleListCmd ??
            (_refreshPeopleListCmd = new RelayCommand(async () =>
            {
                IsLoadingPeople = true;
                RaisePropertyChanged(() => IsLoadingPeople);
                var res = await server.Activities.ListPeople(CurrentActivity.Id);
                if (res.response == StatusCodes.OK)
                {
                    ActivityPeople.Clear();
                    foreach (var item in res.content)
                        ActivityPeople.Add(item);
                }
                IsLoadingPeople = false;
                RaisePropertyChanged(() => IsLoadingPeople);
            }));
    }
}
