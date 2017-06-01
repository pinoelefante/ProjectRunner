using ProjectRunner.ServerAPI;
using ProjectRunner.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace ProjectRunner
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            ViewModelLocator.RegisterPages();

            var cache = ViewModelLocator.GetService<PRCache>();
            if (cache.HasCredentials())
                MainPage = new Views.LoadingPage();
            else
            {
                MainPage = new NavigationPage(new Views.LoginPage());
                ViewModelLocator.NavigationService.Initialize(MainPage as NavigationPage, ViewModelLocator.HomePage);
            }
        }
        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
