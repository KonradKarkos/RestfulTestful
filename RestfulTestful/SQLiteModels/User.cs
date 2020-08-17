using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTestful.SQLiteModels
{
    [Table("User")]
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public long ID { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public long TokenNumber { get; set; }
        public bool Inactive { get; set; }
    }
}