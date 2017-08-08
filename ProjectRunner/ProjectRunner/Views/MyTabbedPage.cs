using ProjectRunner.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ProjectRunner.Views
{
    public class MyTabbedPage : TabbedPage
    {
        public MyTabbedPage(object parameter = null)
        {
            navigationParameter = parameter;
        }
        private MyViewModel VM => this.BindingContext as MyViewModel;
        protected object navigationParameter = null;
        protected override void OnAppearing()
        {
            base.OnAppearing();
            VM?.NavigatedToAsync(navigationParameter);
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            VM?.NavigatedFrom();
        }
        protected override bool OnBackButtonPressed()
        {
            if(VM!=null)
                return VM.OnBackPressed();
            return false;
        }
    }
}
