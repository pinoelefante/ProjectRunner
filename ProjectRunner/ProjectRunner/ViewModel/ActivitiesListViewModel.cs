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
        public ActivitiesListViewModel(INavigationService n, PRServer s)
        {
            navigation = n;
            server = s;
        }
        public override bool OnBackPressed()
        {
            navigation.NavigateTo(ViewModelLocator.HomePage);
            return true;
        }
        public override void NavigatedTo(object parameter = null)
        {
            base.NavigatedTo(parameter);
            LoadMyListAsync();
        }

        public List<Activity> ListActivities { get; } = new List<Activity>();
        public List<Activity> ListPendingActivities { get { return ListActivities.Where(x => x.Status == ActivityStatus.PENDING).ToList(); } }
        public List<Activity> ListMyActivities { get { return ListActivities.Where(x => x.IsMine).ToList(); } }

        private async Task LoadMyListAsync()
        {
            var res = await server.Activities.MyActivitiesListAsync();
            if(res.response == StatusCodes.OK)
            {
                ListActivities.Clear();
                ListActivities.AddRange(res.content);

                RaisePropertyChanged(() => ListPendingActivities);
                RaisePropertyChanged(() => ListMyActivities);
            }
        }

        private RelayCommand _addActivityCmd, _searchActivityCmd;
        private RelayCommand<Activity>_itemTappedCmd, _itemMyTappedCmd;
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
                navigation.NavigateTo(ViewModelLocator.ActivityDetails, (object)x);
            }));
        public RelayCommand<Activity> OpenMyActivityCommand =>
            _itemMyTappedCmd ??
            (_itemMyTappedCmd = new RelayCommand<Activity>((x) =>
            {

            }));
        public RelayCommand SearchActivityCommand =>
            _searchActivityCmd ??
            (_searchActivityCmd = new RelayCommand(() =>
            {

            }));
    }
}
