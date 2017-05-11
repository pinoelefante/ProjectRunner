using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProjectRunner.ServerAPI
{
    public class PRServer
    {
        private CommonServerAPI commonApi { get; }
        public AuthenticationAPI Authentication { get; private set; }
        public ActivityAPI Activities { get; private set; }
        public PRServer()
        {
            commonApi = new CommonServerAPI();
            Authentication = new AuthenticationAPI(commonApi);
            commonApi.SilentLoginAction = () => { return Authentication.SilentLoginAsync().Result; };
            Activities = new ActivityAPI(commonApi);
        }
    }
    public class CommonServerAPI
    {
        private HttpClient http;
        private static readonly string SERVER_ADDRESS = /*"http://localhost";*/"http://gestioneserietv.altervista.org";
        private static readonly string SERVER_ENDPOINT = $"{SERVER_ADDRESS}/prserver";

        public CommonServerAPI()
        {
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler() { CookieContainer = cookieContainer, UseCookies = true, AllowAutoRedirect = false };
            http = new HttpClient(handler);
            http.BaseAddress = new Uri(SERVER_ENDPOINT);
            http.DefaultRequestHeaders.Add("User-Agent", "ProjectRunnerUA");
        }
        
        public bool IsLogged { get; set; } = false;
        public Func<bool> SilentLoginAction { get; set; }
        public Action OnAccessCodeError { get; set; }
        public async Task<Envelop<T>> sendRequest<T>(string url, HttpContent postContent = null, bool loginRequired = true)
        {
            Envelop<T> envelop = new Envelop<T>();
            
            if (loginRequired && !IsLogged)
            {
                if (!SilentLoginAction.Invoke())
                {
                    OnAccessCodeError?.Invoke();
                    return envelop;
                }
            }
            
            try
            {
                Debug.WriteLine(await postContent.ReadAsStringAsync());
                var response = await http.PostAsync($"{SERVER_ENDPOINT}{url}", postContent);
                Debug.WriteLine($"REQUEST at {url} - {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                {
                    var output = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine(output);
                    var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(output);

                    envelop.time = DateTime.Parse(result["time"], CultureInfo.InvariantCulture);
                    envelop.response = (StatusCodes)Enum.ToObject(typeof(StatusCodes), Int32.Parse(result["response"]));
                    Debug.WriteLine("STAUS CODE: " + envelop.response);
                    if (typeof(T) == typeof(string))
                        envelop.content = (T)(object)result["content"];
                    else
                        envelop.content = JsonConvert.DeserializeObject<T>(result["content"]);
                    return envelop;
                }
                else
                {
                    envelop.time = DateTime.Now;
                    envelop.response = StatusCodes.ERRORE_SERVER;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"ERRORE - {e.Message}");
                envelop.time = DateTime.Now;
                envelop.response = StatusCodes.ERRORE_CONNESSIONE;
            }
            return envelop;
        }
        public async Task<Envelop<ContentType>> sendRequestWithAction<ContentType, ContentContainer>(string url, Func<ContentContainer, ContentType> parseAction, HttpContent postContent = null, bool loginRequired = true)
        {
            Envelop<ContentType> envelop = new Envelop<ContentType>();

            if (parseAction == null)
                return await sendRequest<ContentType>(url, postContent, loginRequired);

            if (loginRequired && !IsLogged)
            {
                if (!SilentLoginAction.Invoke())
                {
                    OnAccessCodeError?.Invoke();
                    return envelop;
                }
            }
            try
            {
                var response = await http.PostAsync($"{SERVER_ENDPOINT}{url}", postContent);
                Debug.WriteLine($"REQUEST at {url} - {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                {
                    var output = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine(output);
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(output);

                    envelop.time = DateTime.Parse(result["time"].ToString(), CultureInfo.InvariantCulture);
                    envelop.response = (StatusCodes)Enum.ToObject(typeof(StatusCodes), Int32.Parse(result["response"].ToString()));
                    var values = JsonConvert.DeserializeObject<ContentContainer>(result["content"].ToString());
                    envelop.content = parseAction.Invoke(values);
                    return envelop;
                }
                else
                {
                    envelop.time = DateTime.Now;
                    envelop.response = StatusCodes.ERRORE_SERVER;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"ERRORE - {e.Message}");
                envelop.time = DateTime.Now;
                envelop.response = StatusCodes.ERRORE_CONNESSIONE;
            }
            return envelop;
        }
    }
    public class AuthenticationAPI
    {
        private CommonServerAPI server;
        public AuthenticationAPI(CommonServerAPI client)
        {
            server = client;
        }
        public async Task<bool> SilentLoginAsync()
        {
            var username = "user_test";
            var password = "pass_test";
            var res = await LoginAsync(username, password);
            return res.response == StatusCodes.OK;
        }
        public async Task<Envelop<string>> LoginAsync(string username, string password)
        {
            FormUrlEncodedContent postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("username",username),
                new KeyValuePair<string, string>("password", password)
            });
            var response = await server.sendRequest<string>("/authentication.php?action=Login", postContent, false);
            server.IsLogged = response.response == StatusCodes.OK;
            return response;
        }
        public async Task<Envelop<string>> RegisterAsync(string username, string password, string email, string firstName, string lastName, string birth, string phone)
        {
            FormUrlEncodedContent postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("username",username),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("firstName", firstName),
                new KeyValuePair<string, string>("lastName", lastName),
                new KeyValuePair<string, string>("birth", birth),
                new KeyValuePair<string, string>("phone", phone),
                new KeyValuePair<string, string>("email", email),
            });
            var response = await server.sendRequest<string>("/authentication.php?action=Register", postContent, false);
            server.IsLogged = response.response == StatusCodes.OK;
            return response;
        }
        public async Task<Envelop<string>> ModifyField(string field, string newValue)
        {
            FormUrlEncodedContent postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("field",field),
                new KeyValuePair<string, string>("newValue", newValue)
            });
            return await server.sendRequest<string>("/authentication.php?action=ModifyField", postContent);
        }
        public async Task<Envelop<UserProfile>> GetProfileInfo()
        {
            return await server.sendRequestWithAction<UserProfile, Dictionary<string, string>>("/authentication.php?action=GetProfileInfo", (x) =>
            {
                if (x != null && x.Any())
                {
                    UserProfile user = UserProfile.parseDictionary(x);
                    return user;
                }
                return null;
            });
        }
        public async Task<Envelop<string>> ModifyPassword(string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }
        public async Task<Envelop<string>> Logout()
        {
            throw new NotImplementedException();
        }
        public async Task<Envelop<string>> RecoverPassword()
        {
            throw new NotImplementedException();
        }
    }
    public class ActivityAPI
    {
        private CommonServerAPI server;
        public ActivityAPI(CommonServerAPI s)
        {
            server = s;
        }

        public async Task<Envelop<string>> CreateActivityAsync(DateTime startDay, TimeSpan startTime, int mpPoint, int maxPlayers, int guests, float fee, Sports sport, int feedback, Dictionary<string, string> sportDetails)
        {
            var content = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>(ActivityDatabase.STARTTIME, $"{startDay.Year}-{startDay.Month.ToString("D2")}-{startDay.Day.ToString("D2")} {startTime.Hours.ToString("D2")}:{startTime.Minutes.ToString("D2")}:00"),
                new KeyValuePair<string, string>(ActivityDatabase.MEETINGPOINT, mpPoint.ToString()),
                new KeyValuePair<string, string>(ActivityDatabase.MAXPLAYERS, maxPlayers.ToString()),
                new KeyValuePair<string, string>(ActivityDatabase.GUESTUSERS, guests.ToString()),
                new KeyValuePair<string, string>(ActivityDatabase.FEE, fee.ToString()),
                new KeyValuePair<string, string>(ActivityDatabase.SPORT, ((int)sport).ToString()),
                new KeyValuePair<string, string>(ActivityDatabase.FEEDBACK, feedback.ToString()),
            };

            switch(sport)
            {
                case Sports.BICYCLE:
                    content.AddRange(GetBicycleDetails(sportDetails));
                    break;
                case Sports.FOOTBALL:
                    content.AddRange(GetFootballDetails(sportDetails));
                    break;
                case Sports.RUNNING:
                    content.AddRange(GetRunningDetails(sportDetails));
                    break;
                case Sports.TENNIS:
                    content.AddRange(GetTennisDetails(sportDetails));
                    break;
            }
            FormUrlEncodedContent postContent = new FormUrlEncodedContent(content);
            return await server.sendRequest<string>("/activities.php?action=CreateActivity", postContent);
        }
        private IEnumerable<KeyValuePair<string,string>> GetBicycleDetails(Dictionary<string, string> details)
        {
            var cont = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>($"sportDetails_{BicycleDatabase.DISTANCE}", details[BicycleDatabase.DISTANCE])
            };
            return cont;
        }
        private IEnumerable<KeyValuePair<string, string>> GetFootballDetails(Dictionary<string, string> details)
        {
            var cont = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>($"sportDetails_{FootballDatabase.PLAYERSPERTEAM}", details[FootballDatabase.PLAYERSPERTEAM])
            };
            return cont;
        }
        private IEnumerable<KeyValuePair<string, string>> GetRunningDetails(Dictionary<string, string> details)
        {
            var cont = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>($"sportDetails_{RunningDatabase.DISTANCE}", details[RunningDatabase.DISTANCE]),
                new KeyValuePair<string, string>($"sportDetails_{RunningDatabase.FITNESS}", details[RunningDatabase.FITNESS])
            };
            return cont;
        }
        private IEnumerable<KeyValuePair<string, string>> GetTennisDetails(Dictionary<string, string> details)
        {
            var cont = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>($"sportDetails_{TennisDatabase.DOUBLE}", details[TennisDatabase.DOUBLE])
            };
            return cont;
        }
        public async Task<Envelop<string>> JoinActivityAsync(int activityId)
        {
            var postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>(ActivityDatabase.ID, activityId.ToString())
            });
            return await server.sendRequest<string>("/activities.php?action=JoinActivity", postContent);
        }
        public async Task<Envelop<string>> LeaveActivityAsync(int activityId)
        {
            var postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>(ActivityDatabase.ID, activityId.ToString())
            });
            return await server.sendRequest<string>("/activities.php?action=LeaveActivity", postContent);
        }
        public async Task<Envelop<Activity>> InfoActivityAsync(int activityId)
        {
            var postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>(ActivityDatabase.ID, activityId.ToString())
            });
            return await server.sendRequestWithAction<Activity, Dictionary<string, string>>("/activities.php?action=InfoActivity",(x)=>
            {
                if(x!=null && x.ContainsKey(ActivityDatabase.SPORT))
                {
                    var sport = (Sports)Enum.Parse(typeof(Sports), x[ActivityDatabase.SPORT]);
                    switch(sport)
                    {
                        case Sports.BICYCLE:
                            return BicycleActivity.ParseDictionary(x);
                        case Sports.FOOTBALL:
                            return FootballActivity.ParseDictionary(x);
                        case Sports.RUNNING:
                            return RunningActivity.ParseDictionary(x);
                        case Sports.TENNIS:
                            return TennisActivity.ParseDictionary(x);
                    }
                }
                return null;
            }, postContent);
        }
        public async Task<Envelop<List<Activity>>> MyActivitiesListAsync(Sports? sport = null, ActivityStatus? status = null)
        {
            var parameters = new List<KeyValuePair<string, string>>();
            if (sport != null)
                parameters.Add(new KeyValuePair<string, string>(ActivityDatabase.SPORT, ((int)sport.Value).ToString()));
            if (status != null)
                parameters.Add(new KeyValuePair<string, string>(ActivityDatabase.STATUS, ((int)status.Value).ToString()));
            var postContent = new FormUrlEncodedContent(parameters);

            return await server.sendRequestWithAction<List<Activity>, List<Dictionary<string, string>>>("/activities.php?action=MyActivitiesList", (x) =>
            {
                return ParseDictionaryListActivity(x);
            }, postContent);
        }
        private List<Activity> ParseDictionaryListActivity(List<Dictionary<string,string>> x)
        {
            if (x == null)
                return null;
            List<Activity> activities = new List<Activity>(x.Count);
            foreach (var jsonActivity in x)
            {
                var act = ParseDictionaryActivity(jsonActivity);
                activities.Add(act);
            }
            return activities;
        }
        private Activity ParseDictionaryActivity(Dictionary<string, string> jsonActivity)
        {
            Activity act = null;
            var s = (Sports)Enum.Parse(typeof(Sports), jsonActivity[ActivityDatabase.SPORT]);
            switch (s)
            {
                case Sports.BICYCLE:
                    act = BicycleActivity.ParseDictionary(jsonActivity);
                    break;
                case Sports.FOOTBALL:
                    act = FootballActivity.ParseDictionary(jsonActivity);
                    break;
                case Sports.RUNNING:
                    act = RunningActivity.ParseDictionary(jsonActivity);
                    break;
                case Sports.TENNIS:
                    act = TennisActivity.ParseDictionary(jsonActivity);
                    break;
            }
            return act;
        }
        public async Task<Envelop<string>> DeleteActivityAsync(int activityId)
        {
            var postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>(ActivityDatabase.ID, activityId.ToString())
            });
            return await server.sendRequest<string>("/activities.php?action=DeleteActivity", postContent);
        }
        public async Task<Envelop<string>> ModifyActivityFieldAsync(int activityId, string field, string newValue)
        {
            FormUrlEncodedContent postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>(ActivityDatabase.ID,activityId.ToString()),
                new KeyValuePair<string, string>("field",field),
                new KeyValuePair<string, string>("newValue", newValue)
            });
            return await server.sendRequest<string>("/authentication.php?action=ModifyActivityField", postContent);
        }
        public async Task<Envelop<List<MapAddress>>> ListAddressAsync()
        {
            return await server.sendRequestWithAction<List<MapAddress>, List<Dictionary<string, string>>>("/activities.php?action=ListAddress", (x) =>
            {
                if(x!=null && x.Any())
                {
                    List<MapAddress> addr = new List<MapAddress>(x.Count);
                    foreach (var item in x)
                    {
                        addr.Add(MapAddress.ParseDictionary(item));
                    }
                    return addr;
                }
                return null;
            });
        }
        public async Task<Envelop<string>> AddAddress(string name, float latitude, float longitude)
        {
            FormUrlEncodedContent postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(MapAddressDatabase.NAME, name),
                    new KeyValuePair<string, string>(MapAddressDatabase.LATITUDE,latitude.ToString("N7").Replace(',','.')),
                    new KeyValuePair<string, string>(MapAddressDatabase.LONGITUDE, longitude.ToString("N7").Replace(',','.')),
                });
            return await server.sendRequest<string>("/activities.php?action=AddAddress", postContent);
        }
        public async Task<Envelop<string>> AddAddressPoint(string name, double latitude, double longitude)
        {
            FormUrlEncodedContent postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(MapAddressDatabase.NAME, name),
                    new KeyValuePair<string, string>(MapAddressDatabase.LATITUDE,latitude.ToString("N7").Replace(',','.')),
                    new KeyValuePair<string, string>(MapAddressDatabase.LONGITUDE, longitude.ToString("N7").Replace(',','.')),
                });
            return await server.sendRequest<string>("/activities.php?action=AddAddressPoint", postContent);
        }
        public async Task<Envelop<string>> ReloadLocationInfoFromGoogleMaps(int locationId)
        {
            FormUrlEncodedContent postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(MapAddressDatabase.ID, locationId.ToString()),
                });
            return await server.sendRequest<string>("/activities.php?action=ReloadAddressInfoFromGoogleMaps", postContent);
        }
    }
    public class PeopleAPI
    {
        private CommonServerAPI server;
        public PeopleAPI(CommonServerAPI s)
        {
            server = s;
        }
        public async Task<Envelop<string>> RequestFriendship(int userId)
        {
            throw new NotImplementedException();
        }
        public async Task<Envelop<string>> AcceptFriendship(int userId)
        {
            throw new NotImplementedException();
        }
        public async Task<Envelop<string>> RejectFriendship(int userId)
        {
            throw new NotImplementedException();
        }
        public async Task<Envelop<string>> RemoveFriend(int userId)
        {
            throw new NotImplementedException();
        }
        public async Task<Envelop<bool>> IsFriend(int userId)
        {
            throw new NotImplementedException();
        }
        public async Task<Envelop<List<UserProfile>>> FriendList()
        {
            throw new NotImplementedException();
        }
        public async Task<Envelop<List<UserProfile>>> FriendshipRequested()
        {
            throw new NotImplementedException();
        }
        public async Task<Envelop<List<UserProfile>>> FriendshipReceived()
        {
            throw new NotImplementedException();
        }
    }
    public class UserProfile
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime Birth { get; set; }
        public string Phone { get; set; }
        public DateTime RegistrationTime { get; set; }
        public DateTime LastUpdate { get; set; }

        private UserProfile() { }

        public static UserProfile parseDictionary(Dictionary<string, string> dictionary)
        {
            UserProfile profile = new UserProfile()
            {
                Id = int.Parse(dictionary["id"]),
                Username = dictionary["username"],
                Birth = DateTime.Parse(dictionary["birth"], CultureInfo.InvariantCulture),
                Email = dictionary["email"],
                FirstName = dictionary["firstName"],
                LastName = dictionary["lastName"],
                Phone = dictionary["phone"],
                LastUpdate = DateTime.Parse(dictionary["lastUpdate"], CultureInfo.InvariantCulture),
                RegistrationTime = DateTime.Parse(dictionary["registration"], CultureInfo.InvariantCulture)
            };
            return profile;
        }
    }
    public class MapAddress
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public string Route { get; set; }
        public int StreetNumber { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        public static MapAddress ParseDictionary(Dictionary<string, string> dict, string prefix = "")
        {
            MapAddress mp = new MapAddress()
            {
                Id = Int32.Parse(dict[prefix+MapAddressDatabase.ID]),
                City = dict[prefix + MapAddressDatabase.CITY],
                Country = dict[prefix + MapAddressDatabase.COUNTRY],
                Latitude = float.Parse(dict[prefix + MapAddressDatabase.LATITUDE]),
                Longitude = float.Parse(dict[prefix + MapAddressDatabase.LONGITUDE]),
                Name = dict[prefix + MapAddressDatabase.NAME],
                PostalCode = dict[prefix + MapAddressDatabase.POSTAL_CODE],
                Province = dict[prefix + MapAddressDatabase.PROVINCE],
                Region = dict[prefix + MapAddressDatabase.REGION],
                Route = dict[prefix + MapAddressDatabase.ROUTE],
                StreetNumber = !string.IsNullOrEmpty(dict[prefix + MapAddressDatabase.STREET_NUMBER]) ? Int32.Parse(dict[prefix + MapAddressDatabase.STREET_NUMBER]) : -1
            };
            return mp;
        }
        public override string ToString()
        {
            if(!string.IsNullOrEmpty(Name))
                return Name;
            return $"{Latitude} {Longitude}";
        }
    }
    class MapAddressDatabase
    {
        public const string ID = "id";
        public const string NAME = "name";
        public const string LATITUDE = "latitude";
        public const string LONGITUDE = "longitude";
        public const string ROUTE = "route";
        public const string STREET_NUMBER = "street_number";
        public const string CITY = "city";
        public const string REGION = "region";
        public const string PROVINCE = "province";
        public const string POSTAL_CODE = "postal_code";
        public const string COUNTRY = "country";
    }
    public class Activity
    {
        public int Id { get; set; }
        public int CreatedBy { get; set; }
        public DateTime StartTime { get; set; }
        public MapAddress MeetingPoint {get;set;}
        public int GuestUsers { get; set; }
        public int MaxPlayers { get; set; }
        public int JoinedPlayers { get; set; }
        public ActivityStatus Status { get; set; }
        public Sports Sport { get; set; }
        public float Fee { get; set; }
        public int RequiredFeedback { get; set; }

        public int NumberOfPlayers
        {
            get
            {
                return 1 + GuestUsers + JoinedPlayers;    
            }
        }
        protected Activity() { }
        protected static void ParseDictionary(Activity act, Dictionary<string, string> dict)
        {
            act.CreatedBy = Int32.Parse(dict[ActivityDatabase.CREATEDBY]);
            act.Fee = float.Parse(dict[ActivityDatabase.FEE]);
            act.RequiredFeedback = Int32.Parse(dict[ActivityDatabase.FEEDBACK]);
            act.GuestUsers = Int32.Parse(dict[ActivityDatabase.GUESTUSERS]);
            act.Id = Int32.Parse(dict[ActivityDatabase.ID]);
            act.JoinedPlayers = Int32.Parse(dict["joinedPlayers"]);
            act.MaxPlayers = Int32.Parse(dict[ActivityDatabase.MAXPLAYERS]);
            act.Sport = (Sports)Enum.Parse(typeof(Sports), dict[ActivityDatabase.SPORT]);
            act.StartTime = DateTime.Parse(dict[ActivityDatabase.STARTTIME], CultureInfo.InvariantCulture);
            act.Status = (ActivityStatus)Enum.Parse(typeof(ActivityStatus), dict[ActivityDatabase.STATUS]);
            act.MeetingPoint = MapAddress.ParseDictionary(dict, "mp_");
        }
    }
    public class BicycleActivity : Activity
    {
        public float Distance { get; set; }
        public float Traveled { get; set; }
        private BicycleActivity() { }
        public static Activity ParseDictionary(Dictionary<string,string> dict)
        {
            BicycleActivity act = new BicycleActivity();
            ParseDictionary(act, dict);
            return act;
        }
        public static Dictionary<string, string> CreateDetailsDictionary(float? distance = null, float? traveled = null)
        {
            Dictionary<string, string> details = new Dictionary<string, string>();
            if(distance != null)
                details.Add(BicycleDatabase.DISTANCE, distance.ToString());
            if (traveled != null)
                details.Add(BicycleDatabase.TRAVELED, traveled.ToString());
            return details;
        }
    }
    public class FootballActivity : Activity
    {
        public int PlayersPerTeam { get; set; }
        private FootballActivity() { }
        public static Activity ParseDictionary(Dictionary<string, string> dict)
        {
            FootballActivity act = new FootballActivity();
            ParseDictionary(act, dict);
            return act;
        }
        public static Dictionary<string, string> CreateDetailsDictionary(int? playersTeam = null)
        {
            Dictionary<string, string> details = new Dictionary<string, string>();
            if (playersTeam != null)
                details.Add(FootballDatabase.PLAYERSPERTEAM, playersTeam.ToString());
            return details;
        }
    }
    public class RunningActivity : Activity
    {
        public float Distance { get; set; }
        public float Traveled { get; set; }
        public bool WithFitness { get; set; }
        private RunningActivity() { }
        public static Activity ParseDictionary(Dictionary<string, string> dict)
        {
            RunningActivity act = new RunningActivity();
            ParseDictionary(act, dict);
            return act;
        }
        public static Dictionary<string, string> CreateDetailsDictionary(float? distance = null, float? traveled = null, bool? fitness = null)
        {
            Dictionary<string, string> details = new Dictionary<string, string>();
            if (distance != null)
                details.Add(RunningDatabase.DISTANCE, distance.ToString());
            if (traveled != null)
                details.Add(RunningDatabase.TRAVELED, traveled.ToString());
            if (distance != null)
                details.Add(RunningDatabase.FITNESS, (fitness == true ? "1" : "0"));
            return details;
        }
    }
    public class TennisActivity : Activity
    {
        public bool IsDouble { get; set; }
        private TennisActivity() { }
        public static Activity ParseDictionary(Dictionary<string, string> dict)
        {
            TennisActivity act = new TennisActivity();
            ParseDictionary(act, dict);
            return act;
        }
        public static Dictionary<string, string> CreateDetailsDictionary(bool? _double = null)
        {
            Dictionary<string, string> details = new Dictionary<string, string>();
            if (_double != null)
                details.Add(TennisDatabase.DOUBLE, _double == true ? "1" : "0");
            return details;
        }
    }
    public enum Sports
    {
        RUNNING = 1,
        FOOTBALL = 2,
        BICYCLE = 3,
        TENNIS = 4
    }
    public enum ActivityStatus
    {
        PENDING = 0,
        STARTED = 1,
        ENDED = 2,
        CANCELLED = -1,
        DELETED = -2
    }
    class ActivityDatabase
    {
        public const string ID = "id";
        public const string CREATEDBY = "createdBy";
        public const string STARTTIME = "startTime";
        public const string MEETINGPOINT = "meetingPoint";
        public const string GUESTUSERS = "guestUsers";
        public const string MAXPLAYERS = "maxPlayers";
        public const string STATUS = "status";
        public const string SPORT = "sport";
        public const string FEE = "fee";
        public const string FEEDBACK = "requiredFeedback";
    }
    class BicycleDatabase
    {
        public const string ID = "id_activity";
        public const string DISTANCE = "distance";
        public const string TRAVELED = "traveled";
    }
    class FootballDatabase
    {
        public const string ID = "id_activity";
        public const string PLAYERSPERTEAM = "playersPerTeam";
    }
    class RunningDatabase
    {
        public const string ID = "id_activity";
        public const string DISTANCE = "distance";
        public const string TRAVELED = "traveled";
        public const string FITNESS = "fitness";
    }
    class TennisDatabase
    {
        public const string ID = "id_activity";
        public const string DOUBLE = "isDouble";
    }
}
