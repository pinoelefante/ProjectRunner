using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using ProjectRunner.Services;
using ProjectRunner.Droid.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(SQLiteService_Droid))]
namespace ProjectRunner.Droid.Services
{
    public class SQLiteService_Droid : ISQLite
    {
        public SQLiteService_Droid()
        {
            var sqliteFilename = "projectRunner.sqlite";
            string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal); // Documents folder
            DATABASE_PATH = Path.Combine(documentsPath, sqliteFilename);
        }
        private static string DATABASE_PATH;
        public SQLite.SQLiteConnection GetConnection()
        {
            // Create the connection
            var conn = new SQLite.SQLiteConnection(DATABASE_PATH);
            // Return the database connection
            return conn;
        }
    }
}