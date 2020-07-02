using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTestful.SQLiteModels
{
    [Table("Sale")]
    public class Sale
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        [ForeignKey(typeof(Product))]
        public int ProductID { get; set; }
        [ForeignKey(typeof(Client))]
        public int ClientID { get; set; }
        public int Quantity { get; set; }
        public bool Payed { get; set; }
        public bool Archieved { get; set; }
        public double Discount { get; set; }
        [ManyToOne]
        public Product Product { get; set; }
        [ManyToOne]
        public Client Client { get; set; }
    }
}