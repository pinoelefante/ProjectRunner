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
        public List<UserProfile> ListProfiles { get; } = new List<UserProfile>();

        public bool HasUserProfile(int id)
        {
            return ListProfiles.Where(x => x.Id == id).Any();
        }
        public UserProfile GetUserProfile(int id)
        {
            return ListProfiles.FirstOrDefault(x => x.Id == id);
        }
    }
}
