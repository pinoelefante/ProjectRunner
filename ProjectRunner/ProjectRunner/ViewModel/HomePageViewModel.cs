using GalaSoft.MvvmLight.Command;
using ProjectRunner.ServerAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectRunner.ViewModel
{
    public class HomePageViewModel : MyViewModel
    {
        private PRServer server;
        public HomePageViewModel(PRServer s)
        {
            server = s;
        }
        public RelayCommand TestCommand => new RelayCommand(() =>
        {
            //server.GoogleMaps.GetCoordinatesFromAddress("Santa Maria la Carità", "Via Visitazione", "290", "80050");
        });
    }
}
