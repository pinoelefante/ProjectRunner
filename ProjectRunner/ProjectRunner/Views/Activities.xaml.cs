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
    public partial class Activities : TabbedPage
    {
        public Activities()
        {
            InitializeComponent();
        }
        ActivitiesListViewModel VM => this.BindingContext as ActivitiesListViewModel;
        protected override void OnAppearing()
        {
            base.OnAppearing();
            VM.NavigatedTo();
        }
    }
}
