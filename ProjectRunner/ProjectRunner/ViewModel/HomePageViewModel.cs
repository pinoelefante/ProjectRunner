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
        public RelayCommand TestCommand => new RelayCommand(async () =>
        {
            var res = await server.Activities.MyActivitiesListAsync();
            if(res.response == StatusCodes.OK)
            {
                foreach (var item in res.content)
                {
                    Debug.WriteLine(item.Id);
                }
            }
            else
            {
                Debug.WriteLine("Errore");
            }
        });
    }
}
