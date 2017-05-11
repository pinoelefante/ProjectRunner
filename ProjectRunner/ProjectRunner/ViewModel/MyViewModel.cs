using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectRunner.ViewModel
{
    public class MyViewModel : ViewModelBase
    {
        private bool busyActive;
        public bool IsBusyActive { get { return busyActive; } set { Set(ref busyActive, value); } }

        public virtual void NavigatedTo(object parameter = null)
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
            return true;
        }
    }
}
