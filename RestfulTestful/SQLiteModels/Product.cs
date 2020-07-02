﻿using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTestful.SQLiteModels
{
    [Table("Product")]
    public class Product
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public double Discount { get; set; }
        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<Sale> Sales { get; set; }
    }
}