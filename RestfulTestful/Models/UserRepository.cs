using RestfulTestful.SQLiteModels;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTestful.Models
{
    public class UserRepository : IDisposable
    {
        SQLiteConnection db;
        public UserRepository()
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
        }

        public void Dispose()
        {
            db.Dispose();
        }

        public User ValidateUser(string username, string password)
        {
            return db.Table<User>().FirstOrDefault(u => u.Name.Equals(username) && u.Password.Equals(password));
        }
    }
}