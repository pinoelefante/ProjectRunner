using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using ProjectRunner.Services;
using ProjectRunner.iOS.Services;
using System.Threading;

[assembly: Xamarin.Forms.Dependency(typeof(CloseAppService_iOS))]
namespace ProjectRunner.iOS.Services
{
    public class CloseAppService_iOS : IClosingApp
    {
        public void CloseApp()
        {
            Thread.CurrentThread.Abort();
        }
    }
}
