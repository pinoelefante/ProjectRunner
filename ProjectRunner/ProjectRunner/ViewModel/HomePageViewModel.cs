using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using ProjectRunner.ServerAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ProjectRunner.ViewModel
{
    public class HomePageViewModel : MyViewModel
    {
        private PRServer server;
        private PRCache cache;
        public HomePageViewModel(INavigationService n, PRServer s, PRCache c) : base(n)
        {
            server = s;
            cache = c;
        }
        public override void NavigatedToAsync(object parameter = null)
        {
            Task.Factory.StartNew(() =>
            {
                Device.BeginInvokeOnMainThread(async () => await CheckAppPermissionAsync());
                Device.BeginInvokeOnMainThread(() => LoadActivities());
            });
            
        }
        private async Task CheckAppPermissionAsync()
        {
            await CheckPermissionsAsync(new List<Permission>()
            {
                Permission.Location,
                Permission.Camera,
                Permission.Storage
            });
        }
        private Activity _activitySelected;
        public Activity ActivitySelected { get => _activitySelected; set { Set(ref _activitySelected, value); OnSelectionChanged(); } }
        public ObservableCollection<Activity> PendingActivities { get; } = new ObservableCollection<Activity>();
        private void LoadActivities()
        {
            PendingActivities.Clear();
            var coll = cache.ListActivities.Where(x => x.Status == ActivityStatus.PENDING || x.Status == ActivityStatus.STARTED);
            if (coll != null && coll.Any())
            {
                foreach (var item in coll)
                    PendingActivities.Add(item);
            }
            ActivitySelected = PendingActivities.FirstOrDefault();
        }
        private RelayCommand _startActivityCmd, _finishActivityCmd;
        public RelayCommand StartActivity =>
            _startActivityCmd ??
            (_startActivityCmd = new RelayCommand(async () =>
            {
                if (ActivitySelected == null || ActivitySelected.Status == ActivityStatus.STARTED)
                    return;
                var res = await server.Activities.StartActivityAsync(ActivitySelected.Id);
                if(res.response == StatusCodes.OK)
                {
                    ActivitySelected.Status = ActivityStatus.STARTED;
                    RaisePropertyChanged(() => ActivitySelected);
                    RaisePropertyChanged(() => ActivitySelected.Status);
                }
            }));
        public RelayCommand FinishActivity =>
            _finishActivityCmd ??
            (_finishActivityCmd = new RelayCommand(async () =>
            {
                if (ActivitySelected == null || ActivitySelected.Status == ActivityStatus.ENDED)
                    return;
                var res = await server.Activities.FinishActivityAsync(ActivitySelected.Id);
                if (res.response == StatusCodes.OK)
                {
                    ActivitySelected.Status = ActivityStatus.ENDED;
                    RaisePropertyChanged(() => ActivitySelected);
                    RaisePropertyChanged(() => ActivitySelected.Status);
                }
            }));
        private bool _otherOptions;
        public bool OtherOptionsShowing { get => _otherOptions; set => Set(ref _otherOptions, value); }
        private void OnSelectionChanged()
        {
            OtherOptionsShowing = false;
        }
        private RelayCommand _openOptionsCmd;
        public RelayCommand OpenOtherOptionsCommand =>
            _openOptionsCmd ??
            (_openOptionsCmd = new RelayCommand(() =>
            {
                Debug.WriteLine("Before - OtherOptionsShowing = " + OtherOptionsShowing);
                OtherOptionsShowing = !OtherOptionsShowing;
                Debug.WriteLine("After - OtherOptionsShowing = " + OtherOptionsShowing);
            }));
    }
}
