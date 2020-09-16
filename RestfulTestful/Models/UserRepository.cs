using RestfulTestful.SQLiteModels;
using SQLite;
using System;

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

        public Employee ValidateUser(string username, string password)
        {
            return db.Table<Employee>().FirstOrDefault(e => e.Name.Equals(username) && e.Password.Equals(password) && !e.Inactive);
        }
    }
}