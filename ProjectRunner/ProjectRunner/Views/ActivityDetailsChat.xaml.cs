using ProjectRunner.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ProjectRunner.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ActivityDetailsChat : ContentPage
    {
        public ActivityDetailsChat()
        {
            InitializeComponent();
            (BindingContext as ActivityDetailsViewModel).ScrollToPosition = (x) => 
            {
                if(x!=null)
                    chatList.ScrollTo(x, ScrollToPosition.MakeVisible, true);
            };
        }
    }
}