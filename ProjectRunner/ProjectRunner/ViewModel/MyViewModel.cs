using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Views;
using ProjectRunner.ServerAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectRunner.ViewModel
{
    public class MyViewModel : ViewModelBase
    {
        protected INavigationService navigation;
        public MyViewModel(INavigationService n)
        {
            navigation = n;
        }
        private bool busyActive;
        public bool IsBusyActive { get { return busyActive; } set { Set(ref busyActive, value); } }

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
            return false;
        }
        public bool ManageCommonServerResponse(StatusCodes code)
        {
            return false;
        }
    }
}
