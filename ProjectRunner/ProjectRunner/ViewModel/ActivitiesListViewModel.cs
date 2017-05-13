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

        public ObservableCollection<Activity> ListActivities { get; } = new ObservableCollection<Activity>();
        public List<Activity> ListPendingActivities { get { return ListActivities.Where(x => x.Status == ActivityStatus.PENDING).ToList(); } }
        private async Task LoadMyListAsync()
        {
            var res = await server.Activities.MyActivitiesListAsync();
            if(res.response == StatusCodes.OK)
            {
                ListActivities.Clear();
                foreach (var item in res.content)
                    ListActivities.Add(item);
                RaisePropertyChanged(() => ListPendingActivities);
                Debug.WriteLine(ListActivities.Count + " activities. " + ListPendingActivities + " pending");
            }
        }



        private RelayCommand _addActivityCmd;
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

            }));
    }
}
