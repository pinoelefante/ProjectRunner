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
    public class RegisterPageViewModel : MyViewModel
    {
        private INavigationService navigation;
        private PRServer server;
        private PRCache cache;
        private UserDialogsService dialogs;
        public RegisterPageViewModel(INavigationService n, PRServer s, UserDialogsService d, PRCache c)
        {
            navigation = n;
            server = s;
            dialogs = d;
            cache = c;
        }
        private string _username, _pass1, _pass2, _firstName, _lastName, _email, _phone;
        private DateTime _birth = DateTime.Now;
        public string Username { get { return _username; } set { Set(ref _username, value); } }
        public string Password { get { return _pass1; } set { Set(ref _pass1, value); } }
        public string Password2 { get { return _pass2; } set { Set(ref _pass2, value); } }
        public string FirstName { get { return _firstName; } set { Set(ref _firstName, value); } }
        public string LastName { get { return _lastName; } set { Set(ref _lastName, value); } }
        public string Email { get { return _email; } set { Set(ref _email, value); } }
        public DateTime Birth { get { return _birth; } set { Set(ref _birth, value); } }
        public string Phone { get { return _phone; } set { Set(ref _phone, value); } }

        private RelayCommand _registerCommand;
        public RelayCommand RegisterCommand => _registerCommand ??
            (_registerCommand = new RelayCommand(async () =>
            {
                if (string.IsNullOrEmpty(Password) || Password.CompareTo(Password2) != 0)
                {
                    dialogs.ShowAlert("Invalid password");
                    return;
                }
                var response = await server.Authentication.RegisterAsync(Username, Password, Email, FirstName, LastName, Birth.ToString("yyyy-MM-dd"), Phone);
                if (response.response == StatusCodes.OK)
                {
                    cache.SaveCredentials(Username, Password);
                    Application.Current.MainPage = new Views.MyMasterPage();
                }
                else
                    dialogs.ShowAlert($"An error occurred while creating a new account: {response.response}");
            }));
    }
}
