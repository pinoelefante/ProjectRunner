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
        public override void NavigatedToAsync(object parameter = null)
        {
            if (parameter != null && parameter is Activity)
            {
                IsEditModeEnabled = false;
                var activity = parameter as Activity;
                if (CurrentActivity != null && activity.Id != CurrentActivity.Id)
                {
                    IsPeopleListLoaded = false;
                    UserJoinedActivity = false;
                    ActivityPeople.Clear();
                    RaisePropertyChanged(() => ActivityPeople);
                    ListMessages.Clear();
                }
                CurrentActivity = activity;
                RaisePropertyChanged(() => CurrentActivity);
                RaisePropertyChanged(() => IsEditable);
                LoadPeopleAsync();
            }
            else
                navigation.GoBack();
        }
        public bool IsEditable
        {
            get
            {
                if(CurrentActivity!=null && CurrentActivity.CreatedBy == cache.CurrentUser.Id && CurrentActivity.Status == ActivityStatus.PENDING)
                {
                    switch(CurrentActivity.Sport)
                    {
                        case Sports.BICYCLE:
                        case Sports.FOOTBALL:
                        case Sports.RUNNING:
                            return true;
                    }
                }
                return false;
            }
        }
        private bool _editMode;
        public bool IsEditModeEnabled { get { return _editMode; } set { Set(ref _editMode, value); } }
        private RelayCommand _enableEditModeCmd, _saveChangesCmd, _openMapCmd, _leaveActivityCmd, _deleteActivityCmd, _sendChatMsgCmd, _joinActivityCmd;
        public RelayCommand ToogleEditModeCommand =>
            _enableEditModeCmd ??
            (_enableEditModeCmd = new RelayCommand(() =>
            {
                IsEditModeEnabled = !IsEditModeEnabled;
                if(IsEditModeEnabled)
                    InitEditMode();
            }));
        public RelayCommand OpenMapCommand =>
            _openMapCmd ??
            (_openMapCmd = new RelayCommand(async () =>
            {
                try
                {
                    var mapName = "Rendez-vous point";
                    if (CurrentActivity.CreatedBy == cache.CurrentUser.Id)
                        mapName = CurrentActivity.MeetingPoint.Name;
                    var res = await CrossExternalMaps.Current.NavigateTo(mapName, CurrentActivity.MeetingPoint.Latitude, CurrentActivity.MeetingPoint.Longitude);
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
            (_saveChangesCmd = new RelayCommand(async () =>
            {
                if(CurrentActivity.Sport == Sports.FOOTBALL)
                    NewMaxPlayers = NewPlayersPerTeam[NewPlayersPerTeamIndex] * 2;

                var currentPlayers = CurrentActivity.JoinedPlayers + CurrentActivity.GuestUsers + (CurrentActivity.OrganizerMode ? 0 : 1);
                var totalNewGuests = CurrentActivity.JoinedPlayers + NewGuests[NewGuestsIndex] + (CurrentActivity.OrganizerMode ? 0 : 1);
                if (NewMaxPlayers < currentPlayers)
                {
                    UserDialogs.Instance.Alert("Max players can't be less than people who joined the activity", "Modify cancelled", "OK");
                    return;
                }
                
                if(totalNewGuests > NewMaxPlayers)
                {
                    UserDialogs.Instance.Alert("New guest value should be smaller");
                    return;
                }
                
                var res = await server.Activities.ModifyActivityAsync(CurrentActivity.Id, NewGuests[NewGuestsIndex], NewMaxPlayers);
                if(res.response == StatusCodes.OK)
                {
                    CurrentActivity.GuestUsers = NewGuests[NewGuestsIndex];
                    CurrentActivity.MaxPlayers = NewMaxPlayers;
                    if(CurrentActivity is TeamActivity)
                        (CurrentActivity as TeamActivity).PlayersPerTeam = NewPlayersPerTeam[NewPlayersPerTeamIndex];
                    IsEditModeEnabled = false;
                    RaisePropertyChanged(() => CurrentActivity);
                    UserDialogs.Instance.Alert("Activity modified successfully", "Modify activity", "OK");
                }
                else
                    UserDialogs.Instance.Alert("It was not possible to update the activity", "Modify error", "OK");
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
                        cache.DeleteChatMessages(CurrentActivity.Id);
                        ListMessages.Clear();
                        ActivityPeople.Clear();
                        UserJoinedActivity = false;
                        navigation.GoBack();
                    }
                }

            }));
        public RelayCommand LeaveActivityCommand =>
            _leaveActivityCmd ??
            (_leaveActivityCmd = new RelayCommand(async () =>
            {
                if (CurrentActivity.CreatedBy == cache.CurrentUser.Id)
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
                            if (cache.ListActivities.Remove(cache.ListActivities.First(x=>x.Id == CurrentActivity.Id)))
                                Debug.WriteLine("Item removed from cache");
                            UserJoinedActivity = false;
                            ListMessages.Clear();
                            cache.DeleteChatMessages(CurrentActivity.Id);
                            CurrentActivity.JoinedPlayers--;
                            ActivityPeople.Clear();
                            RaisePropertyChanged(() => CurrentActivity);
                        }
                    }
                }
            }));
        public RelayCommand JoinActivityCommand =>
            _joinActivityCmd ??
            (_joinActivityCmd = new RelayCommand(async () =>
            {
                var totalJoined = CurrentActivity.JoinedPlayers + CurrentActivity.GuestUsers + (CurrentActivity.OrganizerMode ? 0 : 1);
                if (CurrentActivity.Status == ActivityStatus.PENDING && totalJoined < CurrentActivity.MaxPlayers)
                {
                    var res = await server.Activities.JoinActivityAsync(CurrentActivity.Id);
                    if (res.response == StatusCodes.OK)
                    {
                        UserJoinedActivity = true;
                        await LoadPeopleAsync(true);
                        if(cache.ListActivities.Find(x => x.Id == CurrentActivity.Id)==null)
                            cache.ListActivities.Add(CurrentActivity);
                        CurrentActivity.JoinedPlayers++;

                        RaisePropertyChanged(() => CurrentActivity);
                    }
                    else
                    {
                        UserDialogs.Instance.Alert("An error occurred while joining the activity: " + res.response);
                    }
                }
                else
                {
                    if (CurrentActivity.Status != ActivityStatus.PENDING)
                        UserDialogs.Instance.Alert("You can join only pending activities", "");
                    else
                        UserDialogs.Instance.Alert("You can't join this activity", "The activity is full");
                }
            }));
        private string _chatMessage;
        public string ChatMessage { get { return _chatMessage; } set { Set(ref _chatMessage, value); } }
        public RelayCommand SendChatMessageCommand =>
            _sendChatMsgCmd ??
            (_sendChatMsgCmd = new RelayCommand(async () =>
            {
                if (!UserJoinedActivity)
                    return;
                if (ChatMessage?.Trim().Length > 0)
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
                    cache.SetChatLastTimestamp(CurrentActivity.Id, res.content.Last().Timestamp);
                }
            }
        }
        private async Task LoadPeopleAsync(bool force = false)
        {
            IsPeopleListLoaded = false;
            if (!ActivityPeople.Any() || force)
            {
                IsLoadingPeople = true;
                RaisePropertyChanged(() => IsLoadingPeople);
                var res = await server.Activities.ListPeople(CurrentActivity.Id);
                if (res.response == StatusCodes.OK)
                {
                    ActivityPeople.Clear();
                    if(res.content!=null)
                    {
                        foreach (var item in res.content)
                        {
                            ActivityPeople.Add(item);
                            if (!cache.HasUserProfile(item.Id))
                                cache.ListProfiles.Add(item);
                        }
                        cache.SaveItemsDB<UserProfile>(res?.content);
                    }
                }
                IsLoadingPeople = false;
                RaisePropertyChanged(() => IsLoadingPeople);
            }
            if(ActivityPeople.FirstOrDefault(x=>x.Id == cache.CurrentUser.Id) != null)
            {
                UserJoinedActivity = true;
                ReadChatMessagesAsync();
            }
            IsPeopleListLoaded = true;
        }
        private bool _isPeopleListLoaded, _joinedActivity;
        public bool IsPeopleListLoaded { get { return _isPeopleListLoaded; } set { Set(ref _isPeopleListLoaded, value); } }
        public bool UserJoinedActivity { get { return _joinedActivity; } set { Set(ref _joinedActivity, value); } }
        public ObservableCollection<UserProfile> ActivityPeople { get; } = new ObservableCollection<UserProfile>();
        public bool IsLoadingPeople { get; set; }
        private RelayCommand _refreshPeopleListCmd;
        public RelayCommand RefreshPeopleListCommand =>
            _refreshPeopleListCmd ??
            (_refreshPeopleListCmd = new RelayCommand(async () =>
            {
                await LoadPeopleAsync(true);
            }));
        public ObservableCollection<int> NewGuests { get; } = new ObservableCollection<int>();
        private int _newGuestsIndex, _newMaxPlayers, _newPlayersTeamIndex;
        public int NewGuestsIndex { get { return _newGuestsIndex; } set { Set(ref _newGuestsIndex, value); } }
        public int NewMaxPlayers { get { return _newMaxPlayers; } set { Set(ref _newMaxPlayers, value); } }
        public ObservableCollection<int> NewPlayersPerTeam { get; } = new ObservableCollection<int>();
        public int NewPlayersPerTeamIndex { get { return _newPlayersTeamIndex; } set { Set(ref _newPlayersTeamIndex, value); } }
        private void InitEditMode()
        {
            var remainingSpots = CurrentActivity.MaxPlayers - CurrentActivity.JoinedPlayers - (CurrentActivity.OrganizerMode ? 0 : 1);
            NewGuests.Clear();
            for (int i = 0; i <= remainingSpots; i++)
                NewGuests.Add(i);
            NewGuestsIndex = CurrentActivity.GuestUsers;
            RaisePropertyChanged(() => NewGuests);

            NewMaxPlayers = CurrentActivity.MaxPlayers;

            if (CurrentActivity is FootballActivity)
            {
                NewPlayersPerTeam.Clear();
                var minPlayers = (CurrentActivity.JoinedPlayers + CurrentActivity.GuestUsers + 1)/2;
                for(int i = (minPlayers < 5 ? 5 : minPlayers); i<=11;i++)
                    NewPlayersPerTeam.Add(i);
                var currPpt = (CurrentActivity as FootballActivity).PlayersPerTeam;
                NewPlayersPerTeamIndex = (minPlayers < 5) ? currPpt - 5 : currPpt - minPlayers;
            }
        }
    }
}
