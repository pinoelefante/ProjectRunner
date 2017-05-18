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
                var activity = parameter as Activity;
                if (CurrentActivity != null && activity.Id != CurrentActivity.Id)
                {
                    ActivityPeople.Clear();
                    RaisePropertyChanged(() => ActivityPeople);
                    ListMessages.Clear();
                }
                CurrentActivity = activity;
                IsMyActivity = cache.MyUserId == CurrentActivity.CreatedBy;
                RaisePropertyChanged(() => CurrentActivity);
                RaisePropertyChanged(() => IsMyActivity);
                RaisePropertyChanged(() => IsEditModeEnabled);
                LoadPeopleAsync();
                ReadChatMessagesAsync();
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
            (_sendChatMsgCmd = new RelayCommand(async () =>
            {
                if (ChatMessage.Trim().Length > 0)
                {
                    var res = await server.Activities.SendChatMessage(CurrentActivity.Id, ChatMessage.Trim());
                    if (res.response == StatusCodes.OK)
                    {
                        await ReadChatMessagesAsync();
                        ChatMessage = string.Empty;
                    }
                    else
                        UserDialogs.Instance.Alert("Error while sending the message. Retry later", "Message not delivered", "OK");
                }
            }));
        public ObservableCollection<ChatMessage> ListMessages { get; } = new ObservableCollection<ChatMessage>();
        public async Task ReadChatMessagesAsync()
        {
            if (!ListMessages.Any())
            {
                var messages = cache.GetChatMessages(CurrentActivity.Id);
                if (messages != null)
                    foreach (var item in messages)
                        ListMessages.Add(item);
                else
                    cache.SetChatLastTimestamp(CurrentActivity.Id, 0);
            }
            var last_timestamp = cache.GetChatLastTimestamp(CurrentActivity.Id);
            var res = await server.Activities.ReadChatMessages(CurrentActivity.Id, last_timestamp);
            if(res.response == StatusCodes.OK)
            {
                if(res.content.Any())
                {
                    var placeholder = ListMessages.FirstOrDefault(x => x.MessageType == ServerAPI.ChatMessage.ChatMessageType.SERVICE);
                    if (placeholder != null)
                        ListMessages.Remove(placeholder);
                    
                    if(res.content.Count > 1)
                    {
                        ListMessages.Add(new ChatMessage()
                        {
                            Message = $"{res.content.Count} new messages",
                            MessageType = ServerAPI.ChatMessage.ChatMessageType.SERVICE
                        });
                    }
                    foreach (var item in res.content)
                        ListMessages.Add(item);
                    cache.SaveItemsDB<ChatMessage>(res.content);
                    Debug.WriteLine("Last timestamp = " + res.content.Last().Timestamp);
                    cache.SetChatLastTimestamp(CurrentActivity.Id, res.content.Last().Timestamp);
                }
            }
        }
        private async Task LoadPeopleAsync(bool force = false)
        {
            if (!ActivityPeople.Any() || force)
            {
                IsLoadingPeople = true;
                RaisePropertyChanged(() => IsLoadingPeople);
                var res = await server.Activities.ListPeople(CurrentActivity.Id);
                if (res.response == StatusCodes.OK)
                {
                    ActivityPeople.Clear();
                    foreach (var item in res.content)
                    {
                        ActivityPeople.Add(item);
                        if (!cache.HasUserProfile(item.Id))
                            cache.ListProfiles.Add(item);
                    }
                    cache.SaveItemsDB<UserProfile>(res.content);
                }
                IsLoadingPeople = false;
                RaisePropertyChanged(() => IsLoadingPeople);
            }
        }
        public ObservableCollection<UserProfile> ActivityPeople { get; } = new ObservableCollection<UserProfile>();
        public bool IsLoadingPeople { get; set; }
        private RelayCommand _refreshPeopleListCmd;
        public RelayCommand RefreshPeopleListCommand =>
            _refreshPeopleListCmd ??
            (_refreshPeopleListCmd = new RelayCommand(async () =>
            {
                await LoadPeopleAsync(true);
            }));
    }
}
