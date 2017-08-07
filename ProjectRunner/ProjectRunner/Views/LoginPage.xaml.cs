using ProjectRunner.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ProjectRunner.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : MyContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            App.ConfigureNavigation(Application.Current.MainPage as NavigationPage, ViewModelLocator.LoginPage, false);
        }

        private void UsernameEntry_Completed(object sender, EventArgs e)
        {
            passwordEntry.Focus();
        }
    }
}
