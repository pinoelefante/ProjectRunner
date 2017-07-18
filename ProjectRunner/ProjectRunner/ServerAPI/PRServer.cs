using Newtonsoft.Json;
using ProjectRunner.ViewModel;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ProjectRunner.ServerAPI
{
    public class PRServer
    {
        private CommonServerAPI CommonApi { get; }
        public AuthenticationAPI Authentication { get; }
        public ActivityAPI Activities { get; }
        public GoogleMapsAPI GoogleMaps { get; }
        public PRServer(PRCache cache)
        {
            CommonApi = new CommonServerAPI();
            Authentication = new AuthenticationAPI(CommonApi, cache);
            CommonApi.SilentLoginAction = () => { return Authentication.SilentLogin(); };
            CommonApi.OnAccessCodeError = () => 
            {
                Application.Current.MainPage = new Views.LoginPage();
                ViewModelLocator.NavigationService.Initialize(Application.Current.MainPage as NavigationPage, ViewModelLocator.HomePage);
            };
            Activities = new ActivityAPI(CommonApi, cache);
            GoogleMaps = new GoogleMapsAPI(CommonApi);
        }
    }
    public class CommonServerAPI
    {
        private HttpClient http;

        private static readonly string SERVER_ADDRESS = "http://localhost";
        //private static readonly string SERVER_ADDRESS = "http://gestioneserietv.altervista.org";
        private static readonly string SERVER_ENDPOINT = $"{SERVER_ADDRESS}/prserver";

        public CommonServerAPI()
        {
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler() { CookieContainer = cookieContainer, UseCookies = true, AllowAutoRedirect = false };
            http = new HttpClient(handler);
            http.BaseAddress = new Uri(SERVER_ENDPOINT);
            http.DefaultRequestHeaders.Add("User-Agent", "ProjectRunnerUA");
            http.Timeout = TimeSpan.FromSeconds(10);
        }

        public bool IsLogged { get; set; } = false;
        public Func<bool> SilentLoginAction { get; set; }
        public Action OnAccessCodeError { get; set; }
        public async Task<Envelop<T>> SendRequest<T>(string url, HttpContent postContent = null, bool loginRequired = true)
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
                var output = await SendSimpleRequest($"{SERVER_ENDPOINT}{url}", postContent);
                var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(output);

                envelop.time = DateTime.Parse(result["time"], CultureInfo.InvariantCulture);
                envelop.response = (StatusCodes)Enum.ToObject(typeof(StatusCodes), Int32.Parse(result["response"]));
#if DEBUG
                Debug.WriteLine("STAUS CODE: " + envelop.response);
#endif
                if (typeof(T) == typeof(string))
                    envelop.content = (T)(object)result["content"];
                else
                {
                    var json = result["content"].ToString();
                    if (!string.IsNullOrEmpty(json))
                        envelop.content = JsonConvert.DeserializeObject<T>(json);
                }
                return envelop;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"ERROR SendRequest - {e.Message}");
                envelop.time = DateTime.Now;
                envelop.response = StatusCodes.CONNECTION_ERROR;
            }
            return envelop;
        }
        public async Task<Envelop<ContentType>> SendRequestWithAction<ContentType, ContentContainer>(string url, Func<ContentContainer, ContentType> parseAction, HttpContent postContent = null, bool loginRequired = true)
        {
            Envelop<ContentType> envelop = new Envelop<ContentType>();

            if (parseAction == null)
                return await SendRequest<ContentType>(url, postContent, loginRequired);

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
                var output = await SendSimpleRequest($"{SERVER_ENDPOINT}{url}", postContent);
                var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(output);
                envelop.time = DateTime.Parse(result["time"].ToString(), CultureInfo.InvariantCulture);
                envelop.response = (StatusCodes)Enum.ToObject(typeof(StatusCodes), Int32.Parse(result["response"].ToString()));
                var json = result["content"].ToString();
                if (!string.IsNullOrEmpty(json))
                {
                    var values = JsonConvert.DeserializeObject<ContentContainer>(json);
                    envelop.content = parseAction.Invoke(values);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"SendRequestWithAction ERROR - {e.Message}");
                envelop.time = DateTime.Now;
                envelop.response = StatusCodes.CONNECTION_ERROR;
            }
            return envelop;
        }
        public async Task<string> SendSimpleRequest(string url, HttpContent content = null)
        {
            try
            {
#if DEBUG
                if(content!=null)
                    Debug.WriteLine(await content.ReadAsStringAsync());
#endif
                var response = http.PostAsync(url, content).Result;
#if DEBUG
                Debug.WriteLine($"REQUEST at {url} - {response.StatusCode}");
#endif
                if (response.IsSuccessStatusCode)
                {
                    var output = response.Content.ReadAsStringAsync().Result;
#if DEBUG
                    Debug.WriteLine(output);
#endif
                    return output;
                }
            }
            catch(Exception e)
            {
                throw e;
            }
            throw new HttpRequestException();
        }
    }
    public class AuthenticationAPI
    {
        private CommonServerAPI server;
        private PRCache cache;
        public AuthenticationAPI(CommonServerAPI client, PRCache c)
        {
            server = client;
            cache = c;
        }
        public bool SilentLogin()
        {
            var credentials = cache.GetCredentials();
            if (credentials != null)
            {
                var res = LoginAsync(credentials[0], credentials[1]).Result;
                credentials[0] = string.Empty;
                credentials[1] = string.Empty;
                return res.response == StatusCodes.OK;
            }
            return false;
        }
        public async Task<Envelop<string>> LoginAsync(string username, string password)
        {
            FormUrlEncodedContent postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("username",username),
                new KeyValuePair<string, string>("password", password)
            });

            var response = await server.SendRequestWithAction<string, Dictionary<string,string>>("/authentication.php?action=Login", (x)=>
            {
                if (x != null)
                    cache.CurrentUser = UserProfile.ParseDictionary(x);
                return string.Empty;
            }, postContent, false);
            server.IsLogged = response.response == StatusCodes.OK;
            if (server.IsLogged)
                cache.SaveCredentials(username, password);
            return response;
        }
        public async Task<Envelop<string>> RegisterAsync(string username, string password, string email, string firstName, string lastName, string birth, string phone, string timezone)
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
                new KeyValuePair<string, string>("timezone", timezone)
            });
            var response = await server.SendRequest<string>("/authentication.php?action=Register", postContent, false);
            server.IsLogged = response.response == StatusCodes.OK;
            if (server.IsLogged)
                cache.SaveCredentials(username, password);
            return response;
        }
        public async Task<Envelop<string>> ModifyField(string field, string newValue)
        {
            FormUrlEncodedContent postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("field",field),
                new KeyValuePair<string, string>("newValue", newValue)
            });
            return await server.SendRequest<string>("/authentication.php?action=ModifyField", postContent);
        }
        public async Task<Envelop<UserProfile>> GetProfileInfo()
        {
            return await server.SendRequestWithAction<UserProfile, Dictionary<string, string>>("/authentication.php?action=GetProfileInfo", (x) =>
            {
                if (x != null && x.Any())
                {
                    UserProfile user = UserProfile.ParseDictionary(x);
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
            var res = await server.SendRequest<string>("/authentication.php?action=Logout");
            if(res.response == StatusCodes.OK)
                cache.DestroyAll();
            return res;
        }
        public async Task<Envelop<string>> RecoverPassword()
        {
            throw new NotImplementedException();
        }
    }
    public class ActivityAPI
    {
        private CommonServerAPI server;
        private PRCache cache;
        public ActivityAPI(CommonServerAPI s, PRCache c)
        {
            server = s;
            cache = c;
        }

        public async Task<Envelop<string>> CreateActivityAsync(DateTime startDay, TimeSpan startTime, int mpPoint, int maxPlayers, int guests, float fee, string currency, Sports sport, int feedback, Dictionary<string, string> sportDetails)
        {
            var content = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>(ActivityDatabase.STARTTIME, $"{startDay.Year}-{startDay.Month.ToString("D2")}-{startDay.Day.ToString("D2")} {startTime.Hours.ToString("D2")}:{startTime.Minutes.ToString("D2")}:00"),
                new KeyValuePair<string, string>(ActivityDatabase.MEETINGPOINT, mpPoint.ToString()),
                new KeyValuePair<string, string>(ActivityDatabase.MAXPLAYERS, maxPlayers.ToString()),
                new KeyValuePair<string, string>(ActivityDatabase.GUESTUSERS, guests.ToString()),
                new KeyValuePair<string, string>(ActivityDatabase.FEE, fee.ToString()),
                new KeyValuePair<string, string>(ActivityDatabase.CURRENCY, currency),
                new KeyValuePair<string, string>(ActivityDatabase.SPORT, ((int)sport).ToString()),
                new KeyValuePair<string, string>(ActivityDatabase.FEEDBACK, feedback.ToString()),
            };

            switch (sport)
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
            return await server.SendRequest<string>("/activities.php?action=CreateActivity", postContent);
        }
        private IEnumerable<KeyValuePair<string, string>> GetBicycleDetails(Dictionary<string, string> details)
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
            return await server.SendRequest<string>("/activities.php?action=JoinActivity", postContent);
        }
        public async Task<Envelop<string>> LeaveActivityAsync(int activityId)
        {
            var postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>(ActivityDatabase.ID, activityId.ToString())
            });
            
            var response = await server.SendRequest<string>("/activities.php?action=LeaveActivity", postContent);
            if (response.response == StatusCodes.OK)
                cache.DeleteActivity(activityId);
            return response;
        }
        public async Task<Envelop<Activity>> InfoActivityAsync(int activityId)
        {
            var postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>(ActivityDatabase.ID, activityId.ToString())
            });
            return await server.SendRequestWithAction<Activity, Dictionary<string, string>>("/activities.php?action=InfoActivity", (x) =>
             {
                 if (x != null && x.ContainsKey(ActivityDatabase.SPORT))
                 {
                     var sport = (Sports)Enum.Parse(typeof(Sports), x[ActivityDatabase.SPORT]);
                     switch (sport)
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

            return await server.SendRequestWithAction<List<Activity>, List<Dictionary<string, string>>>("/activities.php?action=MyActivitiesList", (x) =>
            {
                return ParseDictionaryListActivity(x);
            }, postContent);
        }
        private List<Activity> ParseDictionaryListActivity(List<Dictionary<string, string>> x)
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
            var response = await server.SendRequest<string>("/activities.php?action=DeleteActivity", postContent);
            if(response.response == StatusCodes.OK)
                cache.DeleteActivity(activityId);
            return response;
        }
        /*
        public async Task<Envelop<string>> ModifyActivityFieldAsync(int activityId, string field, string newValue)
        {
            FormUrlEncodedContent postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>(ActivityDatabase.ID,activityId.ToString()),
                new KeyValuePair<string, string>("field",field),
                new KeyValuePair<string, string>("newValue", newValue)
            });
            return await server.SendRequest<string>("/activities.php?action=ModifyActivityField", postContent);
        }
        */
        public async Task<Envelop<string>> ModifyActivityAsync(int activityId, int newGuests, int newTotalPlayers)
        {
            FormUrlEncodedContent postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>(ActivityDatabase.ID,activityId.ToString()),
                new KeyValuePair<string, string>(ActivityDatabase.GUESTUSERS, newGuests.ToString()),
                new KeyValuePair<string, string>(ActivityDatabase.MAXPLAYERS, newTotalPlayers.ToString())
            });
            return await server.SendRequest<string>("/activities.php?action=ModifyActivity", postContent);
        }
        public async Task<Envelop<List<MapAddress>>> ListAddressAsync()
        {
            return await server.SendRequestWithAction<List<MapAddress>, List<Dictionary<string, string>>>("/activities.php?action=ListAddress", (x) =>
            {
                if (x != null && x.Any())
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
        public async Task<Envelop<MapAddress>> AddAddress(string name, float latitude, float longitude)
        {
            FormUrlEncodedContent postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(MapAddressDatabase.NAME, name),
                    new KeyValuePair<string, string>(MapAddressDatabase.LATITUDE,latitude.ToString("N7").Replace(',','.')),
                    new KeyValuePair<string, string>(MapAddressDatabase.LONGITUDE, longitude.ToString("N7").Replace(',','.')),
                });
            var result = await server.SendRequestWithAction<MapAddress, Dictionary<string, string>>("/activities.php?action=AddAddress", (x) =>
            {
                return x != null && x.Any() ? MapAddress.ParseDictionary(x) : null;
            }, postContent);
            if (result.response == StatusCodes.OK)
                cache.MyMapAddresses.Add(result.content);
            return result;
        }
        public async Task<Envelop<MapAddress>> AddAddressPoint(string name, double latitude, double longitude)
        {
            FormUrlEncodedContent postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(MapAddressDatabase.NAME, name),
                    new KeyValuePair<string, string>(MapAddressDatabase.LATITUDE,latitude.ToString("N7").Replace(',','.')),
                    new KeyValuePair<string, string>(MapAddressDatabase.LONGITUDE, longitude.ToString("N7").Replace(',','.')),
                });
            var result = await server.SendRequestWithAction<MapAddress, Dictionary<string, string>>("/activities.php?action=AddAddressPoint", (x)=>
            {
                return x != null && x.Any() ? MapAddress.ParseDictionary(x) : null;
            }, postContent);
            if (result.response == StatusCodes.OK)
                cache.MyMapAddresses.Add(result.content);
            return result;
        }
        public async Task<Envelop<string>> RemoveAddress(int locationId)
        {
            FormUrlEncodedContent postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(MapAddressDatabase.ID, locationId.ToString())
                });
            var resp = await server.SendRequest<string>("/activities.php?action=RemoveAddress", postContent);
            if (resp.response == StatusCodes.OK)
            {
                var address = cache.MyMapAddresses.Find(x => x.Id == locationId);
                if(address!=null)
                    cache.MyMapAddresses.Remove(address); 
            }
            return resp;
        }
        public async Task<Envelop<string>> ReloadLocationInfoFromGoogleMaps(int locationId)
        {
            FormUrlEncodedContent postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(MapAddressDatabase.ID, locationId.ToString()),
                });
            return await server.SendRequest<string>("/activities.php?action=ReloadAddressInfoFromGoogleMaps", postContent);
        }
        public async Task<Envelop<List<UserProfile>>> ListPeople(int activityId)
        {
            var postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>(ActivityDatabase.ID, activityId.ToString())
            });
            return await server.SendRequestWithAction<List<UserProfile>, List<Dictionary<string, string>>>("/activities.php?action=ListPeople", (x) =>
            {
                if (x != null && x.Any())
                {
                    List<UserProfile> people = new List<UserProfile>(x.Count);
                    foreach (var item in x)
                        people.Add(UserProfile.ParseDictionary(item));
                    return people;
                }
                return null;
            }, postContent);
        }
        public async Task<Envelop<string>> SendChatMessage(int activity, string text)
        {
            var postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>(ChatDatabase.ID_ACTIVITY, activity.ToString()),
                new KeyValuePair<string, string>(ChatDatabase.MESSAGE, text)
            });
            return await server.SendRequest<string>("/activities.php?action=SendChatMessage", postContent);
        }
        public async Task<Envelop<List<ChatMessage>>> ReadChatMessages(int activityId, long timestamp = 0)
        {
            var postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>(ChatDatabase.ID_ACTIVITY, activityId.ToString()),
                new KeyValuePair<string, string>(ChatDatabase.TIMESTAMP, timestamp.ToString())
            });
            return await server.SendRequestWithAction<List<ChatMessage>, List<Dictionary<string, string>>>("/activities.php?action=ReadChatMessages", (x) =>
            {
                if (x != null && x.Any())
                {
                    List<ChatMessage> list = new List<ChatMessage>(x.Count);
                    foreach (var item in x)
                    {
                        ChatMessage message = new ChatMessage()
                        {
                            Message = item[ChatDatabase.MESSAGE],
                            Timestamp = long.Parse(item[ChatDatabase.TIMESTAMP]),
                            ActivityId = activityId,
                            UserId = Int32.Parse(item[ChatDatabase.ID_USER])
                        };
                        message.IsMine = cache.CurrentUser.Id == message.UserId;
                        var user = cache.GetUserProfile(message.UserId);
                        message.SentBy = user ?? new UserProfile() { Id = message.UserId };
                        message.MessageType = ChatMessage.ChatMessageType.USER;

                        list.Add(message);
                    }
                    return list;
                }
                return null;
            }, postContent);
        }
        public async Task<Envelop<List<Activity>>> SearchActivities(Sports sport, double userLatitude, double userLongitude, int mpDistance = 5)
        {
            mpDistance = mpDistance > 50 || mpDistance < -50 ? 50 : mpDistance;
            var postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>(ActivityDatabase.STATUS, ((int)ActivityStatus.PENDING).ToString()),
                new KeyValuePair<string, string>(ActivityDatabase.SPORT, ((int)sport).ToString()),
                new KeyValuePair<string, string>("currentLatitude", userLatitude.ToString()),
                new KeyValuePair<string, string>("currentLongitude", userLongitude.ToString()),
                new KeyValuePair<string, string>("mpDistance", mpDistance.ToString())
            });
            return await server.SendRequestWithAction<List<Activity>, List<Dictionary<string, string>>>("/activities.php?action=SearchActivities", (x)=> 
            {
                return ParseDictionaryListActivity(x);
            }, postContent);
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
        [PrimaryKey]
        public int Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime Birth { get; set; }
        public string Phone { get; set; }
        public DateTime RegistrationTime { get; set; }
        public DateTime LastUpdate { get; set; }
        public int DefaultUserLocation { get; set; }
        public bool NotifyNearbyActivities { get; set; }
        public int Sex { get; set; }
        public string Image { get; set; }

        public UserProfile() { }

        public static UserProfile ParseDictionary(Dictionary<string, string> dictionary)
        {
            UserProfile profile = new UserProfile()
            {
                Id = int.Parse(dictionary["id"]),
                Username = dictionary["username"],
                Email = dictionary.ContainsKey("email") ? dictionary["email"] : string.Empty,
                FirstName = dictionary.ContainsKey("firstName") ? dictionary["firstName"] : string.Empty,
                LastName = dictionary.ContainsKey("lastName") ? dictionary["lastName"] : string.Empty,
                Phone = dictionary.ContainsKey("phone") ? dictionary["phone"] : string.Empty,
                Sex = dictionary.ContainsKey("sex") ? Int32.Parse(dictionary["sex"]) : 0,
                DefaultUserLocation = dictionary.ContainsKey("defaultLocation") && dictionary["defaultLocation"] != null ? Int32.Parse(dictionary["defaultLocation"]) : 0,
                NotifyNearbyActivities = dictionary.ContainsKey("notifyNearbyActivities") ? (Int32.Parse(dictionary["notifyNearbyActivities"]) == 1 ? true : false) : false, 
            };
            if (dictionary.ContainsKey("birth"))
                profile.Birth = DateTime.Parse(dictionary["birth"], CultureInfo.InvariantCulture);
            if(dictionary.ContainsKey("lastUpdate"))
                profile.LastUpdate = DateTime.Parse(dictionary["lastUpdate"], CultureInfo.InvariantCulture);
            if (dictionary.ContainsKey("registration"))
                profile.RegistrationTime = DateTime.Parse(dictionary["registration"], CultureInfo.InvariantCulture);

            return profile;
        }
    }
    public class GoogleMapsAPI
    {
        private CommonServerAPI comm;
        public GoogleMapsAPI(CommonServerAPI c)
        {
            comm = c;
        }
        private static readonly string GOOGLEMAPS_API_KEY = "AIzaSyDVPJKCj8wPi50f1x3BV_rUrOKRaDI6ZXM";
        public Location GetCoordinatesFromAddress(string city, string street, string streetNo, string postalCode)
        {
            var address = (!string.IsNullOrEmpty(street) ? street : "") + (!string.IsNullOrEmpty(streetNo) ? ","+streetNo : "")+
                ", "+(!string.IsNullOrEmpty(postalCode) ? postalCode + " " : "") + (!string.IsNullOrEmpty(city) ? city : "");
            var endpoint = $"https://maps.googleapis.com/maps/api/geocode/json?address={address}&key={GOOGLEMAPS_API_KEY}";
            try
            {
                var output = comm.SendSimpleRequest(endpoint).Result;
                var result = JsonConvert.DeserializeObject<GoogleMapsGeocode>(output);
                if(result.status=="OK" && result.results.Any())
                    return result.results[0].geometry.location;
            }
            catch
            {
                Debug.WriteLine("exception catched");
            }
            return null;
        }
        public class AddressComponent
        {
            public string long_name { get; set; }
            public string short_name { get; set; }
            public List<string> types { get; set; }
        }

        public class Location
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }

        public class Northeast
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }

        public class Southwest
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }

        public class Viewport
        {
            public Northeast northeast { get; set; }
            public Southwest southwest { get; set; }
        }

        public class Geometry
        {
            public Location location { get; set; }
            public string location_type { get; set; }
            public Viewport viewport { get; set; }
        }

        public class Result
        {
            public List<AddressComponent> address_components { get; set; }
            public string formatted_address { get; set; }
            public Geometry geometry { get; set; }
            public string place_id { get; set; }
            public List<string> types { get; set; }
        }

        public class GoogleMapsGeocode
        {
            public List<Result> results { get; set; }
            public string status { get; set; }
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
                Id = Int32.Parse(dict[prefix + MapAddressDatabase.ID]),
                City = dict[prefix + MapAddressDatabase.CITY],
                Country = dict[prefix + MapAddressDatabase.COUNTRY],
                Latitude = float.Parse(dict[prefix + MapAddressDatabase.LATITUDE].Replace(',','.'), CultureInfo.InvariantCulture),
                Longitude = float.Parse(dict[prefix + MapAddressDatabase.LONGITUDE].Replace(',', '.'), CultureInfo.InvariantCulture),
                Name = dict[prefix + MapAddressDatabase.NAME],
                PostalCode = dict[prefix + MapAddressDatabase.POSTAL_CODE],
                Province = dict[prefix + MapAddressDatabase.PROVINCE],
                Region = dict[prefix + MapAddressDatabase.REGION],
                Route = dict[prefix + MapAddressDatabase.ROUTE],
                StreetNumber = !string.IsNullOrEmpty(dict[prefix + MapAddressDatabase.STREET_NUMBER]) ? Int32.Parse(dict[prefix + MapAddressDatabase.STREET_NUMBER]) : -1,
            };
            return mp;
        }
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
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
    public interface TeamActivity
    {
        int PlayersPerTeam { get; set; }
    }
    public interface RoadActivity
    {
        float Distance { get; set; }
        float Travelled { get; set; }
    }
    public class Activity
    {
        public int Id { get; set; }
        public int CreatedBy { get; set; }
        public DateTime StartTime { get; set; }
        public MapAddress MeetingPoint { get; set; }
        public float MPDistance { get; set; }
        public int GuestUsers { get; set; }
        public int MaxPlayers { get; set; }
        public int JoinedPlayers { get; set; }
        public ActivityStatus Status { get; set; }
        public Sports Sport { get; set; }
        public float Fee { get; set; }
        public string Currency { get; set; } = "EUR";
        public int RequiredFeedback { get; set; }
        
        //TODO for IAP
        public bool IsPrivate { get; set; }
        public bool OrganizerMode { get; set; }

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
            act.Fee = float.Parse(dict[ActivityDatabase.FEE].Replace(',', '.'), CultureInfo.InvariantCulture);
            act.Currency = dict[ActivityDatabase.CURRENCY];
            act.RequiredFeedback = Int32.Parse(dict[ActivityDatabase.FEEDBACK]);
            act.GuestUsers = Int32.Parse(dict[ActivityDatabase.GUESTUSERS]);
            act.Id = Int32.Parse(dict[ActivityDatabase.ID]);
            act.JoinedPlayers = Int32.Parse(dict["joinedPlayers"]);
            act.MaxPlayers = Int32.Parse(dict[ActivityDatabase.MAXPLAYERS]);
            act.Sport = (Sports)Enum.Parse(typeof(Sports), dict[ActivityDatabase.SPORT]);
            act.StartTime = DateTime.Parse(dict[ActivityDatabase.STARTTIME], CultureInfo.InvariantCulture);
            act.Status = (ActivityStatus)Enum.Parse(typeof(ActivityStatus), dict[ActivityDatabase.STATUS]);
            act.MeetingPoint = MapAddress.ParseDictionary(dict, "mp_");
            act.MPDistance = dict.ContainsKey("mp_distance") ? float.Parse(dict["mp_distance"].Replace(',', '.'), CultureInfo.InvariantCulture) : 0f;
        }
    }
    public class BicycleActivity : Activity, RoadActivity
    {
        public float Distance { get; set ; }
        public float Travelled { get; set; }

        private BicycleActivity() { }
        public static Activity ParseDictionary(Dictionary<string, string> dict)
        {
            BicycleActivity act = new BicycleActivity();
            ParseDictionary(act, dict);
            act.Distance = float.Parse(dict[BicycleDatabase.DISTANCE].Replace(',', '.'), CultureInfo.InvariantCulture);
            act.Travelled = float.Parse(dict[BicycleDatabase.TRAVELED].Replace(',', '.'), CultureInfo.InvariantCulture);
            return act;
        }
        public static Dictionary<string, string> CreateDetailsDictionary(float? distance = null, float? traveled = null)
        {
            Dictionary<string, string> details = new Dictionary<string, string>();
            if (distance != null)
                details.Add(BicycleDatabase.DISTANCE, distance.ToString());
            if (traveled != null)
                details.Add(BicycleDatabase.TRAVELED, traveled.ToString());
            return details;
        }
    }
    public class FootballActivity : Activity, TeamActivity
    {
        public int PlayersPerTeam { get; set; }
        public FootballActivity() { }
        public static Activity ParseDictionary(Dictionary<string, string> dict)
        {
            FootballActivity act = new FootballActivity();
            ParseDictionary(act, dict);
            act.PlayersPerTeam = Int32.Parse(dict[FootballDatabase.PLAYERSPERTEAM]);
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
    public class RunningActivity : Activity, RoadActivity
    {
        public float Distance { get; set; }
        public float Travelled { get; set; }
        public bool WithFitness { get; set; }

        private RunningActivity() { }
        public static Activity ParseDictionary(Dictionary<string, string> dict)
        {
            RunningActivity act = new RunningActivity();
            ParseDictionary(act, dict);
            act.Distance = float.Parse(dict[RunningDatabase.DISTANCE].Replace(',', '.'), CultureInfo.InvariantCulture);
            act.Travelled = float.Parse(dict[RunningDatabase.TRAVELLED].Replace(',', '.'), CultureInfo.InvariantCulture);
            act.WithFitness = Int32.Parse(dict[RunningDatabase.FITNESS]) == 0 ? false : true;
            return act;
        }
        public static Dictionary<string, string> CreateDetailsDictionary(float? distance = null, float? traveled = null, bool? fitness = null)
        {
            Dictionary<string, string> details = new Dictionary<string, string>();
            if (distance != null)
                details.Add(RunningDatabase.DISTANCE, distance.ToString());
            if (traveled != null)
                details.Add(RunningDatabase.TRAVELLED, traveled.ToString());
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
            act.IsDouble = Int32.Parse(dict[TennisDatabase.DOUBLE]) == 0 ? false : true;
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
    public class ChatMessage
    {
        public int ActivityId { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; }
        [Ignore]
        public UserProfile SentBy { get; set; }
        public bool IsMine { get; set; }
        public long Timestamp { get; set; }
        public ChatMessageType MessageType { get; set; } = ChatMessageType.USER;

        public enum ChatMessageType
        {
            SERVICE,
            USER
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
        public const string CURRENCY = "currency";
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
        public const string TRAVELLED = "traveled";
        public const string FITNESS = "fitness";
    }
    class TennisDatabase
    {
        public const string ID = "id_activity";
        public const string DOUBLE = "isDouble";
    }
    class ChatDatabase
    {
        public const string ID_ACTIVITY = "id_activity",
            ID_USER = "id_user",
            MESSAGE = "message",
            TIMESTAMP = "timestamp";
    }
}
