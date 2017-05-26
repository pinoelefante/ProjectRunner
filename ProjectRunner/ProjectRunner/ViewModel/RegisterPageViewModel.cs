using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using ProjectRunner.ServerAPI;
using ProjectRunner.Services;
using ProjectRunner.Strings;
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
            if(!TimeZones.Any())
                InitTimezones();
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

        public RelayCommand TimeZoneTest =>
            new RelayCommand(() =>
            {
            });
        private int _timezonesContinentIndex = 0, _selectedNation = 0, _cityIndex = 0;
        public int TimezonesContinentIndex { get { return _timezonesContinentIndex; } set { Set(ref _timezonesContinentIndex, value); RaisePropertyChanged(() => CurrentNationsList); } }
        public int TimezonesNationIndex { get { return _selectedNation; } set { Set(ref _selectedNation, value); RaisePropertyChanged(() => CurrentNationCities); } }
        public int TimezonesCityIndex { get { return _cityIndex; } set { Set(ref _cityIndex, value); } }

        public List<string> TimezonesContinent
        {
            get
            {
                return (from kvp in TimeZones select kvp.Key).ToList();
            }
        }
        public List<string> CurrentNationsList
        {
            get
            {
                List<string> result = null;
                switch (TimezonesContinentIndex)
                {
                    case 0:
                        result = (from kvp in TimeZones[TimezonesContinentIndex].Value select kvp.Key).ToList();
                        break;
                    default:
                        return null;
                }
                return result;
            }
        }
        
        public List<string> CurrentNationCities
        {
            get
            {
                var list = (from kvp in TimeZones[TimezonesContinentIndex].Value[TimezonesNationIndex].Value select kvp.Key).ToList();
                TimezonesCityIndex = 0;
                return list;
            }
        }
        public List<KeyValuePair<string, List<KeyValuePair<string, List<KeyValuePair<string, string>>>>>> TimeZones { get; } = new List<KeyValuePair<string, List<KeyValuePair<string, List<KeyValuePair<string, string>>>>>>();
        private void InitTimezones()
        {
            var _europeList = new List<KeyValuePair<string, List<KeyValuePair<string, string>>>>();
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Albania", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Tirana","Europe/Tirane"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Andorra", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Andorra","Europe/Andorra"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Austria", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Vienna","Europe/Vienna"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Bielorussia", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Minsk","Europe/Minsk"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Bosnia ed Erzegovina", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Sarajevo","Europe/Sarajevo"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Bruxelles", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Bruxelles","Europe/Brussels"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Bulgaria", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Sofia","Europe/Sofia"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Città del Vaticano", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Città del Vaticano","Europe/Vatican"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Croazia", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Zagabria","Europe/Zagreb"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Danimarca", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Copenhagen","Europe/Copenhagen"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Estonia", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Tallinn","Europe/Tallinn"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Finlandia", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Helsinki","Europe/Helsinki"),
                new KeyValuePair<string, string>("Mariehamn","Europe/Mariehamn"),
            }.OrderBy(z => z.Key).ToList()));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Francia", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Parigi","Europe/Paris"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Germania", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Berlino","Europe/Berlin"),
                new KeyValuePair<string, string>("Busingen am Hochrhein","Europe/Busingen"),

            }.OrderBy(z => z.Key).ToList()));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Principato di Monaco", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Monaco","Europe/Monaco"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Regno Unito", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Gibilterra","Europe/Gibraltar"),
                new KeyValuePair<string, string>("Guernsey","Europe/Guernsey"),
                new KeyValuePair<string, string>("Isola di Man","Europe/Isle_of_Man"),
                new KeyValuePair<string, string>("Jersey","Europe/Jersey"),
                new KeyValuePair<string, string>("Londra","Europe/London"),
                new KeyValuePair<string, string>("Belfast","Europe/Belfast"),
            }.OrderBy(x => x.Key).ToList()));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Grecia", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Atene","Europe/Athens"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Irlanda", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Dublino","Europe/Dublin"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Italia", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Roma","Europe/Rome"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Lettonia", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Riga","Europe/Riga"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Liechtenstein", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Vaduz","Europe/Vaduz"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Lituania", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Vilnius","Europe/Vilnius"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Lussemburgo", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Lussemburgo","Europe/Luxembourg"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Macedonia", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Skopje","Europe/Skopje"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Malta", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("La Valletta","Europe/Malta"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Cipro", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Nicosia","Europe/Nicosia"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Moldavia", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Chisinau","Europe/Chisinau"),
                new KeyValuePair<string, string>("Tiraspol","Europe/Tiraspol"),
            }.OrderBy(z => z.Key).ToList()));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Montenegro", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Podgorica","Europe/Podgorica"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Norvegia", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Oslo","Europe/Oslo"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Olanda", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Amsterdam","Europe/Amsterdam"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Polonia", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Varsavia","Europe/Warsaw"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Portogallo", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Lisbona","Europe/Lisbon"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Repubblica Ceca", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Praga","Europe/Prague"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Romania", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Bucarest","Europe/Bucharest"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Russia", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Kaliningrad","Europe/Kaliningrad"),
                new KeyValuePair<string, string>("Mosca","Europe/Moscow"),
                new KeyValuePair<string, string>("Samara","Europe/Samara"),
                new KeyValuePair<string, string>("Sinferopoli","Europe/Simferopol"),
                new KeyValuePair<string, string>("Volgograd","Europe/Volgograd"),
            }.OrderBy(x => x.Key).ToList()));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Ucraina", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Kiev","Europe/Kiev"),
                new KeyValuePair<string, string>("Uzhorod","Europe/Uzhgorod"),
                new KeyValuePair<string, string>("Zaporizzja","Europe/Zaporozhye"),
                new KeyValuePair<string, string>("Sinferopoli","Europe/Simferopol"),
            }.OrderBy(x => x.Key).ToList()));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("San Marino", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("San Marino","Europe/San_Marino"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Serbia", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Belgrado","Europe/Belgrade"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Slovacchia", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Bratislava","Europe/Bratislava"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Slovenia", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Lubiana","Europe/Ljubljana"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Spagna", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Madrid","Europe/Madrid"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Svezia", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Stoccolma","Europe/Stockholm"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Svizzera", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Zurigo","Europe/Zurich"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>("Turchia", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Istanbul","Europe/Istanbul"),
            }));
            _europeList.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>(TimezonesLocations.EUROPE_HUNGARY, new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>(TimezonesLocations.HUNGARY_BUDAPEST,"Europe/Budapest")
            }));
            _europeList = _europeList.OrderBy(x => x.Key).ToList();

            TimeZones.Add(new KeyValuePair<string, List<KeyValuePair<string, List<KeyValuePair<string, string>>>>>(TimezonesLocations.CONTINENT_EUROPE, _europeList));
        }
    }
}
