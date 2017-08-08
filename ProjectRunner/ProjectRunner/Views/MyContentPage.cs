using ProjectRunner.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ProjectRunner.Views
{
    public class MyContentPage : ContentPage
    {
        public MyContentPage(object parameter = null)
        {
            navigationParameter = parameter;
        }
        protected MyViewModel ViewModel => this.BindingContext as MyViewModel;
        protected object navigationParameter = null;
        protected override void OnAppearing()
        {
            base.OnAppearing();
            ViewModel?.NavigatedToAsync(navigationParameter);
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            ViewModel?.NavigatedFrom();
        }
        protected override bool OnBackButtonPressed()
        {
            if (ViewModel!=null)
                return ViewModel.OnBackPressed();
            return false;
        }
    }
}
