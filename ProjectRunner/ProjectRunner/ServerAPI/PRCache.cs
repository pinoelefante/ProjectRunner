using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectRunner.ServerAPI
{
    public class PRCache
    {
        public int MyUserId { get; set; }
        public List<Activity> ListActivities { get; } = new List<Activity>();
    }
}
