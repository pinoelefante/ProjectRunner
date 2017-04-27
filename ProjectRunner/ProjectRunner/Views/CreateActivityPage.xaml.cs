using ProjectRunner.ServerAPI;
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
    public partial class CreateActivityPage : MyContentPage
    {
        public CreateActivityPage()
        {
            InitializeComponent();
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case nameof(viewModel.IsDouble):
                case nameof(viewModel.PlayersPerTeam):
                case nameof(viewModel.SelectedSport):
                    PopulateGuestList();
                    break;
            }
        }

        private CreateActivityViewModel viewModel => this.BindingContext as CreateActivityViewModel;
        private void PopulateGuestList()
        {
            var lastIndex = viewModel.SelectedIndexGuestList;
            int maxPlayers = 0;
            switch(viewModel.SelectedSport)
            {
                case Sports.FOOTBALL:
                    maxPlayers = (viewModel.PlayersPerTeam * 2) - 1;
                    break;
                case Sports.TENNIS:
                    maxPlayers = viewModel.IsDouble ? 3 : 1;
                    break;
            }
            listGuests.Items.Clear();
            for (int i = 0; i <= maxPlayers; i++)
                listGuests.Items.Add(i.ToString());
            if (lastIndex <= maxPlayers)
                viewModel.SelectedIndexGuestList = lastIndex;
            else
                viewModel.SelectedIndexGuestList = 0;
        }
        public class PickerItemSport
        {
            public Sports IdSport { get; set; }
            public string SportName { get; set; }
            public override string ToString()
            {
                return SportName;
            }
        }
    }
}
