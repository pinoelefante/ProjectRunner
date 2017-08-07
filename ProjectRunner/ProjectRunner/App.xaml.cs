using Acr.UserDialogs;
using ProjectRunner.ServerAPI;
using ProjectRunner.Services;
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
                MainPage = new NavigationPage(new Views.LoginPage());
        }
        public static void ConfigureNavigation(NavigationPage nav, string homePageKey, bool askToClose = false)
        {
            IClosingApp closer = ViewModelLocator.GetService<IClosingApp>();
            Action act = async () =>
            {
                if (askToClose)
                {
                    if (!await UserDialogs.Instance.ConfirmAsync("Would you like to close the app?", "Closing app"))
                        return;
                }
                closer.CloseApp();
            };
            ViewModelLocator.NavigationService.Initialize(nav, homePageKey, act);
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
