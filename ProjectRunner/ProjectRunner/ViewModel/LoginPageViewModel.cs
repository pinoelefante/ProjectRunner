using Acr.UserDialogs;
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
    public class LoginPageViewModel : MyViewModel
    {
        private PRServer server;
        private PRCache cache;
        public LoginPageViewModel(INavigationService nav, PRServer s, PRCache c) : base(nav)
        {
            server = s;
            cache = c;
        }
        public override void NavigatedToAsync(object parameter = null)
        {
            if (cache.HasCredentials())
            {
                var credentials = cache.GetCredentials();
                Username = credentials[0];
                Password = credentials[1];
            }
        }

        private string _username, _password;
        public string Username { get => _username; set => Set(ref _username, value); }
        public string Password { get => _password; set => Set(ref _password, value); }

        private RelayCommand _loginCommand, _registerCommand, _forgotPasswordCommand;
        public RelayCommand LoginCommand => _loginCommand ??
            (_loginCommand = new RelayCommand(async () =>
            {
                IsBusyActive = true;
                var result = await server.Authentication.LoginAsync(Username, Password);
                if(result.response == StatusCodes.OK)
                    Application.Current.MainPage = new Views.LoadingPage();
                else
                    UserDialogs.Instance.Alert("Username or password is wrong", "Login failed");
                IsBusyActive = false;
            }));
        public RelayCommand RegisterCommand => _registerCommand ??
            (_registerCommand = new RelayCommand(() =>
            {
                navigation.NavigateTo(ViewModelLocator.RegisterPage);
            }));
        public RelayCommand ForgotPasswordCommand => _forgotPasswordCommand ??
            (_forgotPasswordCommand = new RelayCommand(() =>
            {
                navigation.NavigateTo(ViewModelLocator.ForgotPassword);
            }));
    }
}
