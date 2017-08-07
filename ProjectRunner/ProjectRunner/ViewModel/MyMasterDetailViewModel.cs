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
        public PRCache cache { get; }
        public MyMasterDetailViewModel(INavigationService n, PRCache c) : base(n)
        {
            cache = c;
        }
        public override void NavigatedToAsync(object parameter = null)
        {

        }
        public void Navigate(string pageKey)
        {
            navigation.NavigateTo(pageKey);
        }
        private RelayCommand _openProfile;
        public RelayCommand CloseMasterPage { get; set; }
        public RelayCommand OpenUserProfile =>
            _openProfile ??
            (_openProfile = new RelayCommand(() =>
            {
                navigation.NavigateTo(ViewModelLocator.ViewUserProfile, cache.CurrentUser.Id);
                CloseMasterPage?.Execute(null);
            }));
    }
}
