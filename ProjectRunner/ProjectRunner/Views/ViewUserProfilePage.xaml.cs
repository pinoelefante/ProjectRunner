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
    public partial class ViewUserProfilePage : MyContentPage
    {
        public ViewUserProfilePage(object parameter = null) : base(parameter)
        {
            InitializeComponent();
        }
    }
}