using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using ProjectRunner.ServerAPI;
using ProjectRunner.Services;
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
    public class ActivitiesListViewModel : MyViewModel
    {
        private PRServer server;
        private PRCache cache;
        public ActivitiesListViewModel(INavigationService n, PRServer s, PRCache c) : base(n)
        {
            server = s;
            cache = c;
        }
        public override void NavigatedToAsync(object parameter = null)
        {
            base.NavigatedToAsync(parameter);
            LoadMyListAsync();
        }
        
        public MyObservableCollection<Activity> ListPendingActivities { get; } = new MyObservableCollection<Activity>();
        public MyObservableCollection<Activity> ListMyActivities { get; } = new MyObservableCollection<Activity>();

        private async Task LoadMyListAsync(bool forced = false)
        {
            if (!cache.ListActivities.Any() || forced)
            {
                IsBusyActive = true;
                Debug.WriteLine("Loading activities from web");
                var res = await server.Activities.MyActivitiesListAsync();
                if (res.response == StatusCodes.OK)
                {
                    ListPendingActivities.AddRange(res.content?.Where(x => x.Status == ActivityStatus.PENDING), true);
                    ListMyActivities.AddRange(res.content?.Where(x => x.CreatedBy == cache.CurrentUser.Id), true);
                    cache.ListActivities.Clear();
                    cache.ListActivities.AddRange(res.content);
                }
                IsBusyActive = false;
            }
            else
            {
                Debug.WriteLine("Loading activities from cache");
                ListPendingActivities.AddRange(cache.ListActivities?.Where(x => x.Status == ActivityStatus.PENDING), true);
                ListMyActivities.AddRange(cache.ListActivities?.Where(x => x.CreatedBy == cache.CurrentUser.Id), true);
            }
        }

        private RelayCommand _addActivityCmd, _searchActivityCmd, _refreshActivitiesCmd;
        private RelayCommand<Activity>_itemTappedCmd;
        public RelayCommand AddActivityCommand =>
            _addActivityCmd ??
            (_addActivityCmd = new RelayCommand(() =>
            {
                navigation.NavigateTo(ViewModelLocator.CreateActivity);
            }));
        public RelayCommand<Activity> OpenActivityCommand =>
            _itemTappedCmd ??
            (_itemTappedCmd = new RelayCommand<Activity>((x) =>
            {
                navigation.NavigateTo(ViewModelLocator.ActivityDetails, x);
            }));
        public RelayCommand SearchActivityCommand =>
            _searchActivityCmd ??
            (_searchActivityCmd = new RelayCommand(() =>
            {
                navigation.NavigateTo(ViewModelLocator.ActivitySearch);
            }));
        public RelayCommand RefreshActivitiesCommand =>
            _refreshActivitiesCmd ??
            (_refreshActivitiesCmd = new RelayCommand(async () =>
            {
                await LoadMyListAsync(true);
            }));
    }
}
