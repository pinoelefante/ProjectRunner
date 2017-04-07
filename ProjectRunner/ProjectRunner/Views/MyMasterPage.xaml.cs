﻿using ProjectRunner.ViewModel;
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
    public partial class MyMasterPage : MasterDetailPage
    {
        public MyMasterPage()
        {
            InitializeComponent();
            masterPage.ListView.ItemTapped += ListView_ItemTapped;
            VM.ConfigureNavigation(navigationPage);
        }
        private MyMasterDetailViewModel VM => this.BindingContext as MyMasterDetailViewModel;
        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var item = e.Item as MasterPageItem;
            VM.Navigate(item.PageKey);
            IsPresented = false;
        }
    }
}
