using ProjectRunner.Services;
using System;
using System.Collections.Generic;
using System.Text;
using SQLite;
using ProjectRunner.iOS.Services;
using Xamarin.Forms;
using System.IO;

[assembly : Dependency(typeof(SQLiteService_iOS))]
namespace ProjectRunner.iOS.Services
{
    public class SQLiteService_iOS : ISQLite
    {
        public SQLiteService_iOS()
        {
            var sqliteFilename = "projectRunner.sqlite";
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); // Documents folder
            string libraryPath = Path.Combine(documentsPath, "..", "Library"); // Library folder
            DATABASE_PATH = Path.Combine(libraryPath, sqliteFilename);
        }
        private static string DATABASE_PATH;
        public SQLiteConnection GetConnection()
        {
            // Create the connection
            var conn = new SQLite.SQLiteConnection(DATABASE_PATH);
            // Return the database connection
            return conn;
        }
    }
}
