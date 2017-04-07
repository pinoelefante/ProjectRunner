using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;

namespace ProjectRunner.Services
{
    public class UserDialogsService
    {
        public void ShowAlert(string message, string title = "", string ok = "OK")
        {
            UserDialogs.Instance.Alert(message, title, ok);
        }
    }
}
