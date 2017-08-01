using Plugin.SecureStorage;
using Plugin.SecureStorage.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectRunner.ServerAPI
{
    public class PRSettings
    {
        private ISecureStorage storage = CrossSecureStorage.Current;

        public MapAddress DefaultAddress { get; set; }
        public string TimeZone { get { return storage.GetValue("timezone", "Europe/London"); } set { storage.SetValue("timezone", value); } }
        public bool NotifyNewActivityNearbyDefaultAddress { get { return bool.Parse(storage.GetValue("notifyNearbyActivities", "false")); } set { storage.SetValue("notifyNearbyActivities", value.ToString()); } }
        public string UnitMeasure
        {
            get
            {
                if (storage.HasKey("UnitMeasure"))
                    return storage.GetValue("UnitMeasure");
                return RegionInfo.CurrentRegion.IsMetric ? "KM" : "MI";
            }
        }
    }
}
