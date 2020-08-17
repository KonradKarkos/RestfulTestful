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
        public long ID { get; set; }
        [ForeignKey(typeof(Product))]
        public long ProductID { get; set; }
        [ForeignKey(typeof(Client))]
        public long ClientID { get; set; }
        public long Quantity { get; set; }
        public bool Payed { get; set; }
        public bool Archieved { get; set; }
        public double Discount { get; set; }
        public DateTime SaleDate { get; set; }
        public DateTime AddDate { get; set; }
        public long TokenNumber { get; set; }
        [ManyToOne]
        public Product Product { get; set; }
        [ManyToOne]
        public Client Client { get; set; }
    }
}