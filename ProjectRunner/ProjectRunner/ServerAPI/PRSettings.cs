using Plugin.SecureStorage;
using Plugin.SecureStorage.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectRunner.ServerAPI
{
    public class PRSettings
    {
        public MapAddress DefaultAddress { get; set; }
        public float TimeZone { get { return float.Parse(storage.GetValue("timezone", "0")); } set { storage.SetValue("timezone", value.ToString()); } }
        public bool NotifyNewActivityNearbyDefaultAddress { get { return bool.Parse(storage.GetValue("notifyNearbyActivities","false")); } set { storage.SetValue("notifyNearbyActivities", value.ToString()); } }

        private ISecureStorage storage = CrossSecureStorage.Current;
        public PRSettings()
        {
            LoadSettings();
        }
        public void LoadSettings()
        {
            TimeZone = float.Parse(storage.GetValue("timezone", "0"));
        }
    }
}
