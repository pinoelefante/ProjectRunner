using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;
using ProjectRunner.ServerAPI;
using ProjectRunner.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ProjectRunner.ViewModel
{
    public class ViewModelLocator
    {
        public const string HomePage = "HomePage",
            LoginPage = "LoginPage",
            RegisterPage = "RegisterPage",
            ForgotPassword = "ForgotPasswordPage",
            CreateActivity = "CreateActivityPage",
            CreateActivityChooseLocation = "CreateActivityChooseLocation",
            CreateActivityAddLocation = "CreateActivityAddLocation",
            CreateActivityConfirm = "CreateActivityConfirm";
        
        private static NavigationService nav;
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            nav = new NavigationService();
            SimpleIoc.Default.Register<INavigationService>(()=>nav);
            SimpleIoc.Default.Register<PRServer>();
            SimpleIoc.Default.Register<UserDialogsService>();

            SimpleIoc.Default.Register<LoginPageViewModel>();
            SimpleIoc.Default.Register<RegisterPageViewModel>();
            SimpleIoc.Default.Register<ForgotPasswordViewModel>();
            SimpleIoc.Default.Register<MyMasterDetailViewModel>();
            SimpleIoc.Default.Register<CreateActivityViewModel>();
            SimpleIoc.Default.Register<HomePageViewModel>();
        }
        public static void RegisterPages()
        {
            nav.Configure(ForgotPassword, typeof(Views.ForgotPasswordPage));
            nav.Configure(LoginPage, typeof(Views.LoginPage));
            nav.Configure(RegisterPage, typeof(Views.RegisterPage));
            nav.Configure(HomePage, typeof(Views.HomePage));

            nav.Configure(CreateActivity, typeof(Views.CreateActivityPage));
            nav.Configure(CreateActivityAddLocation, typeof(Views.CreateActivityAddLocation));
            nav.Configure(CreateActivityChooseLocation, typeof(Views.CreateActivityChooseLocation));
            nav.Configure(CreateActivityConfirm, typeof(Views.CreateActivityConfirmPage));
        }
        public static T GetService<T>() => ServiceLocator.Current.GetInstance<T>();
        public static NavigationService NavigationService { get { return nav; } }

        public LoginPageViewModel LoginPageViewModel => GetService<LoginPageViewModel>();
        public RegisterPageViewModel RegisterPageViewModel => GetService<RegisterPageViewModel>();
        public ForgotPasswordViewModel ForgotPasswordViewModel => GetService<ForgotPasswordViewModel>();
        public MyMasterDetailViewModel MyMasterDetailViewModel => GetService<MyMasterDetailViewModel>();
        public CreateActivityViewModel CreateActivityViewModel => GetService<CreateActivityViewModel>();
        public HomePageViewModel HomePageViewModel => GetService<HomePageViewModel>();
    }
}
