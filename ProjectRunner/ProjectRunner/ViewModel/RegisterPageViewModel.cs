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
        private PRServer server;
        private PRCache cache;
        public RegisterPageViewModel(INavigationService n, PRServer s, PRCache c) : base(n)
        {
            server = s;
            cache = c;
        }
        public DateTime MaxDateTime { get; } = DateTime.Now.Subtract(TimeSpan.FromDays(365.25 * 15));
        public DateTime MinDateTime { get; } = DateTime.Now.Subtract(TimeSpan.FromDays(365.25 * 100));
        private string _username, _pass1, _firstName, _lastName, _email, _phone;
        private DateTime _birth = DateTime.Now.Subtract(TimeSpan.FromDays(365.25 * 18));
        private int _timezoneIndex = 0, _sexIndex = 0;
        public string Username { get => _username; set => Set(ref _username, value); }
        public string Password { get => _pass1; set => Set(ref _pass1, value); }
        public string FirstName { get => _firstName; set => Set(ref _firstName, value); }
        public string LastName { get => _lastName; set => Set(ref _lastName, value); }
        public string Email { get => _email; set => Set(ref _email, value); }
        public DateTime Birth { get => _birth; set => Set(ref _birth, value); }
        public string Phone { get => _phone; set => Set(ref _phone, value); }
        public int TimezoneIndex { get => _timezoneIndex; set { Set(ref _timezoneIndex, value); RaisePropertyChanged(() => TimezoneSelected); } }
        public MyObservableCollection<KeyValuePair<string,string>> Timezones { get; } = new MyObservableCollection<KeyValuePair<string,string>>();
        public int SexIndex { get => _sexIndex; set => Set(ref _sexIndex, value); }
        public List<string> SexList { get; } = new List<string>()
        {
            "Male", "Female"
        };

        private RelayCommand _registerCommand;
        public RelayCommand RegisterCommand => _registerCommand ??
            (_registerCommand = new RelayCommand(async () =>
            {
                if (string.IsNullOrEmpty(Password) || Password.Trim().Length<8)
                {
                    UserDialogs.Instance.Alert("Password should have at least 8 characters");
                    return;
                }
                var response = await server.Authentication.RegisterAsync(Username, Password, Email, FirstName, LastName, Birth.ToString("yyyy-MM-dd"), Phone, TimezoneSelected, SexIndex);
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
            Debug.WriteLine("Rome translation = " + TimezonesLocations.ResourceManager.GetString("Europe__Rome"));

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
                var list = new List<KeyValuePair<string,string>>(res.content.Count());
                foreach (var item in res.content)
                {
                    var name = TimezonesLocations.ResourceManager.GetString(item.Replace("/", "__").Replace("-", "_minus_"));
                    list.Add(new KeyValuePair<string, string>(item, name));
                }
                list = list.OrderBy(x => x.Value).ToList();
                Timezones.AddRange(list);

                RaisePropertyChanged(() => TimezoneSelected);
                RaisePropertyChanged(() => Timezones);
                TimezoneIndex = 0;
            }
            IsTimezoneLoaded = true;
        }
        public string TimezoneSelected { get => TimezoneIndex >=0 && TimezoneIndex < Timezones.Count ? Timezones[TimezoneIndex].Key : "UTC"; }
        private bool _timezoneLoaded = false;
        public bool IsTimezoneLoaded { get => _timezoneLoaded; set => Set(ref _timezoneLoaded, value); }
    }
}
