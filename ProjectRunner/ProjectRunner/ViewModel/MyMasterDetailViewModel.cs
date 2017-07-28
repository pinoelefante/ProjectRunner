using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using ProjectRunner.ServerAPI;
using ProjectRunner.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ProjectRunner.ViewModel
{
    public class MyMasterDetailViewModel : MyViewModel
    {
        private INavigationService navigation;
        private PRCache cache;
        public MyMasterDetailViewModel(INavigationService n, PRCache c)
        {
            navigation = n;
            cache = c;
        }
        public UserProfile User { get; set; }
        public override void NavigatedToAsync(object parameter = null)
        {
            User = cache.CurrentUser;
            RaisePropertyChanged(() => User);
        }
        public void Navigate(string pageKey)
        {
            navigation.NavigateTo(pageKey);
        }
        public void ConfigureNavigation(NavigationPage navPage)
        {
            (navigation as NavigationService).Initialize(navPage, ViewModelLocator.HomePage);
        }
        private RelayCommand _openProfile;
        public RelayCommand CloseMasterPage { get; set; }
        public RelayCommand OpenUserProfile =>
            _openProfile ??
            (_openProfile = new RelayCommand(() =>
            {
                navigation.NavigateTo(ViewModelLocator.ViewUserProfile, User.Id);
                CloseMasterPage?.Execute(null);
            }));
    }
}
