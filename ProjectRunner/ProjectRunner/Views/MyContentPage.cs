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
        private MyViewModel VM => this.BindingContext as MyViewModel;
        private object navigationParameter = null;
        protected override void OnAppearing()
        {
            base.OnAppearing();
            VM.NavigatedTo(navigationParameter);
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            VM.NavigatedFrom();
        }
    }
}
