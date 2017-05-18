using Plugin.SecureStorage;
using Plugin.SecureStorage.Abstractions;
using ProjectRunner.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectRunner.ServerAPI
{
    public class PRCache
    {
        private ISecureStorage storage;
        private ISQLite db;
        public PRCache(ISQLite d)
        {
            storage = CrossSecureStorage.Current;
            db = d;

            InitDB();
        }
        public int MyUserId { get; set; }
        public List<Activity> ListActivities { get; } = new List<Activity>();
        public List<UserProfile> ListProfiles { get; } = new List<UserProfile>();

        public bool HasUserProfile(int id)
        {
            return ListProfiles.Where(x => x.Id == id).Any();
        }
        public UserProfile GetUserProfile(int id)
        {
            var res = ListProfiles.FirstOrDefault(x => x.Id == id);
            if (res == null)
            {
                using (var conn = db.GetConnection())
                {
                    res = conn.Table<UserProfile>().Where(x => x.Id == id).FirstOrDefault();
                }
            }
            return res;
        }
        public long GetChatLastTimestamp(int idActivity)
        {
            return long.Parse(storage.GetValue($"chat_timestamp_{idActivity}", "0"));
        }
        public void SetChatLastTimestamp(int activityId, long timestamp)
        {
            storage.SetValue($"chat_timestamp_{activityId}", timestamp.ToString());
        }
        private void InitDB()
        {
            using (var conn = db.GetConnection())
            {
                conn.CreateTable<UserProfile>();
                conn.CreateTable<ChatMessage>();
            }
        }
        public List<ChatMessage> GetChatMessages(int activityId)
        {
            List<ChatMessage> messages = null;
            using(var con = db.GetConnection())
            {
                messages = con.Table<ChatMessage>().Where(x => x.ActivityId == activityId)?.OrderBy(x => x.Timestamp)?.ToList();
                foreach (var message in messages)
                    message.SentBy = GetUserProfile(message.UserId);
            }
            return messages;
        }
        public void SaveItemDB<T>(T item)
        {
            try
            {
                using (var con = db.GetConnection())
                {
                    con.Insert(item, typeof(T));
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
                UpdateItemDB<T>(item);
            }
        }
        public void SaveItemsDB<T>(IEnumerable<T> items)
        {
            try
            {
                using (var con = db.GetConnection())
                {
                    con.InsertAll(items, typeof(T));
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
                UpdateItemsDB<T>(items);
            }
        }
        public void UpdateItemDB<T>(T item)
        {
            try
            {
                using (var con = db.GetConnection())
                {
                    con.Update(item, typeof(T));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
        public void UpdateItemsDB<T>(IEnumerable<T> items)
        {
            try
            {
                using (var con = db.GetConnection())
                {
                    con.UpdateAll(items);
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
        public void SaveCredentials(string username, string password)
        {
            storage.SetValue("pr_username", username);
            storage.SetValue("pr_password", password);
        }
        public string[] GetCredentials()
        {
            string[] credentials = null;
            if (HasCredentials())
            {
                credentials = new string[2];
                credentials[0] = storage.GetValue("pr_username");
                credentials[1] = storage.GetValue("pr_password");
            }
            return credentials;
        }
        public bool HasCredentials()
        {
            return storage.HasKey("pr_username") && storage.HasKey("pr_password");
        }
    }
}
