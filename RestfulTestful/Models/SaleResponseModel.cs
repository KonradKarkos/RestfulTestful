using RestfulTestful.SQLiteModels;
using System;
using System.Collections.Generic;
using System.Web.Http.Routing;

namespace RestfulTestful.Models
{
    public class SaleResponseModel
    {
        public long ID { get; set; }
        public long ProductID { get; set; }
        public long ClientID { get; set; }
        public long Quantity { get; set; }
        public bool Payed { get; set; }
        public bool Archieved { get; set; }
        public double Discount { get; set; }
        public DateTime SaleDate { get; set; }
        public DateTime AddDate { get; set; }
        public long TokenNumber { get; set; }
        public ProductResponseModel Product { get; set; }
        public ClientResponseModel Client { get; set; }
        public List<HATEOASLinkResponseModel> Links { get; set; }
        public SaleResponseModel(Sale sale, UrlHelper urlHelper, bool makeLinksForChildren)
        {
            ID = sale.ID;
            ProductID = sale.ProductID;
            ClientID = sale.ClientID;
            Quantity = sale.Quantity;
            Payed = sale.Payed;
            Archieved = sale.Archieved;
            Discount = sale.Discount;
            SaleDate = sale.SaleDate;
            AddDate = sale.AddDate;
            TokenNumber = sale.TokenNumber;
            if (makeLinksForChildren && sale.Product != null)
            {
                Product = new ProductResponseModel(sale.Product, urlHelper);
            }
            else if (sale.Product != null)
            {
                Product = new ProductResponseModel(sale.Product);
            }
            if (makeLinksForChildren && sale.Client != null)
            {
                Client = new ClientResponseModel(sale.Client, urlHelper);
            }
            else if(sale.Client != null)
            {
                Client = new ClientResponseModel(sale.Client);
            }
            Links = new List<HATEOASLinkResponseModel>()
            {
                new HATEOASLinkResponseModel(urlHelper.Link("HATEOAS", new {controller = "sales", id = ID}), "self", "GET"),
                new HATEOASLinkResponseModel(urlHelper.Link("HATEOAS", new {controller = "sales", id = ID}), "update_sale", "PUT"),
                new HATEOASLinkResponseModel(urlHelper.Link("HATEOAS", new {controller = "sales", id = ID}), "partially_update_sale", "PATCH"),
                new HATEOASLinkResponseModel(urlHelper.Link("HATEOAS", new {controller = "sales", id = ID}), "archieve_or_delete_archieved_sale", "DELETE")
            };
        }
    }
}