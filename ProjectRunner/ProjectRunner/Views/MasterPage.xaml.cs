﻿using ProjectRunner.ServerAPI;
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
    public partial class MasterPage : ContentPage
    {
        public ListView ListView { get { return listView; } }
        public MasterPage()
        {
            InitializeComponent();
            ListView.ItemsSource = new List<MasterPageItem>()
            {
                new MasterPageItem()
                {
                    Title = "Home",
                    PageKey = ViewModelLocator.HomePage,
                    //IconSource = "home.png"
                },
                new MasterPageItem()
                {
                    Title = "Activities",
                    PageKey = ViewModelLocator.Activities
                }
            };
        }

        private void Logout_Command(object sender, EventArgs e)
        {
            var server = ViewModelLocator.GetService<PRServer>();
            var res = server.Authentication.Logout().Result;
            if(res.response == StatusCodes.OK)
            {
                Application.Current.MainPage = new NavigationPage(new Views.LoginPage());
                ViewModelLocator.NavigationService.Initialize(Application.Current.MainPage as NavigationPage, ViewModelLocator.HomePage);
            }
        }
    }
}
