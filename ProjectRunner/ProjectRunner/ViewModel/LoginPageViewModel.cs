﻿using GalaSoft.MvvmLight;
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
        private INavigationService navigation;
        private PRServer server;
        private PRCache cache;
        private UserDialogsService dialogs;
        public LoginPageViewModel(INavigationService nav, PRServer s, UserDialogsService d, PRCache c)
        {
            navigation = nav;
            server = s;
            dialogs = d;
            cache = c;
        }

        private string _username = "pinoelefante", _password = "elefante";
        public string Username { get { return _username; } set { Set(ref _username, value); } }
        public string Password { get { return _password; } set { Set(ref _password, value); } }

        private RelayCommand _loginCommand, _registerCommand, _forgotPasswordCommand;
        public RelayCommand LoginCommand => _loginCommand ??
            (_loginCommand = new RelayCommand(async () =>
            {
                var result = await server.Authentication.LoginAsync(Username, Password);
                if(result.response == StatusCodes.OK)
                {
                    cache.MyUserId = Int32.Parse(result.content);
                    Application.Current.MainPage = new Views.MyMasterPage();
                }
                else
                {
                    dialogs.ShowAlert("Username or password is wrong", "Login failed");
                }
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
