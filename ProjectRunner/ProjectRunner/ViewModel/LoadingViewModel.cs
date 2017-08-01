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
        public double Progress { get { return _prog; } set { Set(ref _prog, value); } }
        private string _progText;
        public string ProgressText { get { return _progText; } set { Set(ref _progText, value); } }
        private PRCache cache;
        private PRServer server;
        public LoadingViewModel(PRServer s, PRCache c) : base(null)
        {
            server = s;
            cache = c;
        }
        public override void NavigatedToAsync(object parameter = null)
        {
            DoLoadingAsync().ContinueWith((x)=>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (x.Result)
                        Application.Current.MainPage = new Views.MyMasterPage();
                    else
                    {
                        Application.Current.MainPage = new NavigationPage(new Views.LoginPage());
                        ViewModelLocator.NavigationService.Initialize(Application.Current.MainPage as NavigationPage, ViewModelLocator.HomePage);
                    }
                });
            });
        }
        private async Task<bool> DoLoadingAsync()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Progress = 0.1f;
                ProgressText = "Loading addresses";
            });
            var loginRes = await server.Authentication.LoginAsync();
            if(loginRes.response != StatusCodes.OK)
                return false;

            var resAddr = await server.Activities.ListAddressAsync();
            if (resAddr.response == StatusCodes.OK && resAddr.content != null)
                cache.MyMapAddresses.AddRange(resAddr.content);

            Device.BeginInvokeOnMainThread(() =>
            {
                Progress = 0.25f;
                ProgressText = "Loading friends";
            });

            Device.BeginInvokeOnMainThread(() =>
            {
                Progress = 0.5f;
                ProgressText = "Loading friendship requests";
            });

            Device.BeginInvokeOnMainThread(() =>
            {
                Progress = 0.75f;
                ProgressText = "Loading activities";
            });
            var resAct = await server.Activities.MyActivitiesListAsync();
            if(resAct.response==StatusCodes.OK && resAct.content!=null)
                cache.ListActivities.AddRange(resAct.content);
            Device.BeginInvokeOnMainThread(() =>
            {
                Progress = 1f;
                ProgressText = "Loading completed";
            });
            await Task.Delay(250);
            return true;
        }
    }
}
