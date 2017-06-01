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
        private INavigationService navigation;
        private PRServer server;
        private PRCache cache;
        public ActivitiesListViewModel(INavigationService n, PRServer s, PRCache c)
        {
            navigation = n;
            server = s;
            cache = c;
        }
        public override bool OnBackPressed()
        {
            navigation.NavigateTo(ViewModelLocator.HomePage);
            return true;
        }
        public override void NavigatedToAsync(object parameter = null)
        {
            base.NavigatedToAsync(parameter);
            LoadMyListAsync();
        }

        public List<Activity> ListActivities { get; } = new List<Activity>();
        public List<Activity> ListPendingActivities { get { return ListActivities.Where(x => x.Status == ActivityStatus.PENDING).ToList(); } }
        public List<Activity> ListMyActivities { get { return ListActivities.Where(x => x.CreatedBy == cache.CurrentUser.Id).ToList(); } }

        private async Task LoadMyListAsync(bool forced = false)
        {
            if (!cache.ListActivities.Any() || forced)
            {
                IsBusyActive = true;
                Debug.WriteLine("Loading activities from web");
                var res = await server.Activities.MyActivitiesListAsync();
                if (res.response == StatusCodes.OK)
                {
                    ListActivities.Clear();
                    cache.ListActivities.Clear();
                    if (res.content != null)
                    {
                        ListActivities.AddRange(res.content);
                        cache.ListActivities.AddRange(res.content);
                    }
                }
                IsBusyActive = false;
            }
            else
            {
                Debug.WriteLine("Loading activities from cache");
                ListActivities.Clear();
                ListActivities.AddRange(cache.ListActivities);
            }
            RaisePropertyChanged(() => ListPendingActivities);
            RaisePropertyChanged(() => ListMyActivities);
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
