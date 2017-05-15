using ProjectRunner.ServerAPI;
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
    public partial class ActivityDetails : MyTabbedPage
    {
        public ActivityDetails(object parameter)
        {
            InitializeComponent();
            navigationParameter = parameter;
        }
    }
}