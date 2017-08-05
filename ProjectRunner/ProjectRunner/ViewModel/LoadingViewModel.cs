using ProjectRunner.ServerAPI;
using ProjectRunner.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ProjectRunner.ViewModel
{
    public class LoadingViewModel : MyViewModel
    {
        private double _prog = 0.1f;
        public double Progress { get => _prog; set => Set(ref _prog, value); }
        private string _progText;
        public string ProgressText { get => _progText; set => Set(ref _progText, value); }
        private PRCache cache;
        private PRServer server;
        public LoadingViewModel(PRServer s, PRCache c) : base(null)
        {
            server = s;
            cache = c;
        }
        public override void NavigatedToAsync(object parameter = null)
        {
            Task.Factory.StartNew(async () =>
            {
                var res = await DoLoadingAsync();
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (res)
                        Application.Current.MainPage = new MyMasterPage();
                    else
                    {
                        Application.Current.MainPage = new NavigationPage(new LoginPage());
                        ViewModelLocator.NavigationService.Initialize(Application.Current.MainPage as NavigationPage, ViewModelLocator.HomePage);
                    }
                });
            });
        }
        private async Task<bool> DoLoadingAsync()
        {
            UpdateProgress(0.1f, "Loading user profile");
            var loginRes = await server.Authentication.LoginAsync();
            if(loginRes.response != StatusCodes.OK)
            {
                if (loginRes.response == StatusCodes.LOGIN_ERROR)
                    cache.DeleteCredentials();
                return false;
            }

            UpdateProgress(0.3f, "Loading addresses");
            var resAddr = await server.Activities.ListAddressAsync();
            if (resAddr.response == StatusCodes.OK && resAddr.content != null)
                cache.MyMapAddresses.AddRange(resAddr.content);

            UpdateProgress(0.5f, "Loading friends");

            UpdateProgress(0.75f, "Loading activities");
            var resAct = await server.Activities.MyActivitiesListAsync();
            if(resAct.response==StatusCodes.OK && resAct.content!=null)
                cache.ListActivities.AddRange(resAct.content);

            UpdateProgress(1f, "Loading completed");
            await Task.Delay(250);
            return true;
        }
        private void UpdateProgress(double percent, string description = "")
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Progress = percent;
                ProgressText = description;
            });
        }
    }
}
