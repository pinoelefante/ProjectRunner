using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Views;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using ProjectRunner.ServerAPI;
using ProjectRunner.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectRunner.ViewModel
{
    public class MyViewModel : ViewModelBase
    {
        protected NavigationService navigation;
        public MyViewModel(INavigationService n)
        {
            navigation = n as NavigationService;
        }
        private bool busyActive;
        public bool IsBusyActive { get => busyActive; set => Set(ref busyActive, value); }

        public virtual void NavigatedToAsync(object parameter = null)
        {

        }
        public virtual void NavigatedFrom()
        {

        }
        /*
         * OnBackPressed() must return true when override 
         */
        public virtual bool OnBackPressed()
        {
            navigation.GoBack();
            return true;
        }
        public bool ManageCommonServerResponse(StatusCodes code)
        {
            return false;
        }

        public async Task CheckPermissionsAsync(List<Permission> permissions)
        {
            for (int i = 0; i < permissions.Count;)
            {
                if (await CrossPermissions.Current.CheckPermissionStatusAsync(permissions[i]) == PermissionStatus.Granted)
                    permissions.RemoveAt(i);
                else
                    i++;
            }
            if(permissions.Any())
                await CrossPermissions.Current.RequestPermissionsAsync(permissions.ToArray());
        }
        public async Task<bool> CheckPermissionAsync(Permission p)
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(p);
                if (status != PermissionStatus.Granted)
                {

                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(p))
                    {
                        CrossPermissions.Current.OpenAppSettings();
                    }

                    var results = await CrossPermissions.Current.RequestPermissionsAsync(p);
                    //Best practice to always check that the key exists
                    if (results.ContainsKey(p))
                        status = results[p];
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            var status1 = await CrossPermissions.Current.CheckPermissionStatusAsync(p);
            return status1 == PermissionStatus.Granted;
        }
    }
}
