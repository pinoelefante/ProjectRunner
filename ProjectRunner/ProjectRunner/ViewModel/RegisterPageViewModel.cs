using Acr.UserDialogs;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using ProjectRunner.ServerAPI;
using ProjectRunner.Services;
using ProjectRunner.Strings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
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
        public RegisterPageViewModel(INavigationService n, PRServer s, PRCache c)
        {
            navigation = n;
            server = s;
            cache = c;
        }
        private string _username, _pass1, _pass2, _firstName, _lastName, _email, _phone;
        private DateTime _birth = DateTime.Now;
        private int _timezoneIndex = 0;
        public string Username { get { return _username; } set { Set(ref _username, value); } }
        public string Password { get { return _pass1; } set { Set(ref _pass1, value); } }
        public string FirstName { get { return _firstName; } set { Set(ref _firstName, value); } }
        public string LastName { get { return _lastName; } set { Set(ref _lastName, value); } }
        public string Email { get { return _email; } set { Set(ref _email, value); } }
        public DateTime Birth { get { return _birth; } set { Set(ref _birth, value); } }
        public string Phone { get { return _phone; } set { Set(ref _phone, value); } }
        public int TimezoneIndex { get { return _timezoneIndex; } set { Set(ref _timezoneIndex, value); RaisePropertyChanged(() => TimezoneSelected); } }
        public ObservableCollection<string> Timezones { get; } = new ObservableCollection<string>();

        private RelayCommand _registerCommand;
        public RelayCommand RegisterCommand => _registerCommand ??
            (_registerCommand = new RelayCommand(async () =>
            {
                if (string.IsNullOrEmpty(Password) || Password.Trim().Length<8)
                {
                    UserDialogs.Instance.Alert("Password should have at least 8 characters");
                    return;
                }
                var response = await server.Authentication.RegisterAsync(Username, Password, Email, FirstName, LastName, Birth.ToString("yyyy-MM-dd"), Phone, null);
                if (response.response == StatusCodes.OK)
                    Application.Current.MainPage = new Views.MyMasterPage();
                else
                    UserDialogs.Instance.Alert($"An error occurred while creating a new account: {response.response}");
            }));
        public override void NavigatedToAsync(object parameter = null)
        {
            LoadTimezonesAsync();
        }
        public async Task LoadTimezonesAsync()
        {
            IsTimezoneLoaded = false;
            TimezoneIndex = 0;
            Timezones.Clear();
            var regionCode = RegionInfo.CurrentRegion.TwoLetterISORegionName;
            var timezoneOffset = TimeZoneInfo.Local.BaseUtcOffset.TotalSeconds + (TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now) ? 3600 : 0);
            //var regionCode = "US";
            //var timezoneOffset = -14400;
            var res = await server.Authentication.GetTimezones(regionCode, Convert.ToInt32(timezoneOffset));
            if(res.response == StatusCodes.OK)
            {
                foreach (var item in res.content)
                    Timezones.Add(item);
                RaisePropertyChanged(() => TimezoneSelected);
                RaisePropertyChanged(() => Timezones);
                TimezoneIndex = 0;
            }
            IsTimezoneLoaded = true;
        }
        public string TimezoneSelected { get { return TimezoneIndex >=0 && TimezoneIndex < Timezones.Count ? Timezones[TimezoneIndex] : "Europe/London"; } }
        private bool _timezoneLoaded = false;
        public bool IsTimezoneLoaded { get { return _timezoneLoaded; } set { Set(ref _timezoneLoaded, value); } }
    }
}
