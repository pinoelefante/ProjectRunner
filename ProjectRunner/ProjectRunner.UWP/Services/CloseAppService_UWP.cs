using ProjectRunner.Services;
using ProjectRunner.UWP.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(CloseAppService_UWP))]
namespace ProjectRunner.UWP.Services
{
    public class CloseAppService_UWP : IClosingApp
    {
        public void CloseApp()
        {
            App.Current.Exit();
        }
    }
}
