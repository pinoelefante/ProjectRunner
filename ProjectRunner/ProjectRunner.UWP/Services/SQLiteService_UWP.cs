using ProjectRunner.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using ProjectRunner.UWP.Service;
using Xamarin.Forms;
using System.IO;
using Windows.Storage;

[assembly: Dependency(typeof(SQLiteService_UWP))]
namespace ProjectRunner.UWP.Service
{
    public class SQLiteService_UWP : ISQLite
    {
        public SQLiteService_UWP() { }
        private static readonly string DATABASE = Path.Combine(ApplicationData.Current.LocalFolder.Path, "projectRunner.sqlite");
        public SQLiteConnection GetConnection()
        {
            var conn = new SQLiteConnection(DATABASE);
            return conn;
        }
    }
}
