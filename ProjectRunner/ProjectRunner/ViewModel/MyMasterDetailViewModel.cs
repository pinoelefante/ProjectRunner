using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using ProjectRunner.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ProjectRunner.ViewModel
{
    public class MyMasterDetailViewModel : MyViewModel
    {
        private INavigationService navigation;
        public MyMasterDetailViewModel(INavigationService n)
        {
            navigation = n;
        }
        public void Navigate(string pageKey)
        {
            navigation.NavigateTo(pageKey);
        }
        public void ConfigureNavigation(NavigationPage navPage)
        {
            (navigation as NavigationService).Initialize(navPage, ViewModelLocator.HomePage);
        }
    }
}
