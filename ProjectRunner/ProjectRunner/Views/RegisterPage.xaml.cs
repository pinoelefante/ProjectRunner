﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ProjectRunner.Views
{
    public partial class RegisterPage : MyContentPage
    {
        public RegisterPage()
        {
            InitializeComponent();
            entryUsername.Completed += (s, e) => entryPassword.Focus();
            entryPassword.Completed += (s, e) => entryFirstName.Focus();
            entryFirstName.Completed += (s, e) => entryLastName.Focus();
            entryLastName.Completed += (s, e) => sexPicker.Focus();
            entryMail.Completed += (s, e) => entryPhone.Focus();
            entryPhone.Completed += (s, e) => entryBirth.Focus();
        }
    }
}
