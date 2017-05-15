using Acr.UserDialogs;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Plugin.ExternalMaps;
using ProjectRunner.ServerAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectRunner.ViewModel
{
    public class ActivityDetailsViewModel : MyViewModel
    {
        private PRServer server;
        private INavigationService navigation;
        public ActivityDetailsViewModel(PRServer s, INavigationService n)
        {
            server = s;
            navigation = n;
        }
        public Activity CurrentActivity { get; private set; }
        public override void NavigatedTo(object parameter = null)
        {
            if (parameter != null && parameter is Activity)
            {
                IsEditModeEnabled = false;
                CurrentActivity = parameter as Activity;
                RaisePropertyChanged(() => CurrentActivity);
                RaisePropertyChanged(() => IsEditModeEnabled);
            }
            else
                navigation.NavigateTo(ViewModelLocator.Activities);
        }
        public bool IsEditModeEnabled { get; set; }
        private RelayCommand _enableEditModeCmd, _saveChangesCmd, _openMapCmd, _leaveActivityCmd, _deleteActivityCmd;
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
                var res = await CrossExternalMaps.Current.NavigateTo("Rendez-vous point", CurrentActivity.MeetingPoint.Latitude, CurrentActivity.MeetingPoint.Longitude);
                if(!res)
                {
                    //TODO handle error
                    Debug.WriteLine("Map error!");
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
                if(await UserDialogs.Instance.ConfirmAsync("Are you sure to delete this activity?", "Delete activity", "Yes", "No", cts))
                {
                    var res = await server.Activities.DeleteActivityAsync(CurrentActivity.Id);
                    if (res.response == StatusCodes.OK)
                    {
                        navigation.GoBack();
                    }
                }
            }));
        public RelayCommand LeaveActivityCommand =>
            _leaveActivityCmd ??
            (_leaveActivityCmd = new RelayCommand(async () =>
            {
                CancellationToken cts = new CancellationToken();
                if (await UserDialogs.Instance.ConfirmAsync("Are you sure to leave this activity?", "Leave activity", "Yes", "No", cts))
                {
                    var res = await server.Activities.LeaveActivityAsync(CurrentActivity.Id);
                    if (res.response == StatusCodes.OK)
                    {
                        navigation.GoBack();
                    }
                }
            }));
    }
}
