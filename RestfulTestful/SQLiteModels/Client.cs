using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Web;

namespace RestfulTestful.SQLiteModels
{
    [Table("Client")]
    public class Client
    {
        [PrimaryKey, AutoIncrement]
        public long ID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public long PhoneNumber { get; set; }
        public string Address { get; set; }
        public bool Deleted { get; set; }
        public long TokenNumber { get; set; }
        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<Sale> Sales { get; set; }
    }
}