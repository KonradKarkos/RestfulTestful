using RestfulTestful.SQLiteModels;
using System.Collections.Generic;
using System.Web.Http.Routing;

namespace RestfulTestful.Models
{
    public class ProductResponseModel
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public long Quantity { get; set; }
        public double Price { get; set; }
        public double Discount { get; set; }
        public bool Discontinued { get; set; }
        public long TokenNumber { get; set; }
        public List<Sale> Sales { get; set; }
        public List<HATEOASLinkResponseModel> Links { get; set; }
        public ProductResponseModel(Product product)
        {
            ID = product.ID;
            Name = product.Name;
            Category = product.Category;
            Quantity = product.Quantity;
            Price = product.Price;
            Discount = product.Discount;
            Discontinued = product.Discontinued;
            TokenNumber = product.TokenNumber;
            Sales = product.Sales;
        }
        public ProductResponseModel(Product product, UrlHelper urlHelper)
        {
            ID = product.ID;
            Name = product.Name;
            Category = product.Category;
            Quantity = product.Quantity;
            Price = product.Price;
            Discount = product.Discount;
            Discontinued = product.Discontinued;
            TokenNumber = product.TokenNumber;
            Sales = product.Sales;
            Links = new List<HATEOASLinkResponseModel>()
            {
                new HATEOASLinkResponseModel(urlHelper.Link("HATEOAS", new {controller = "products", id = ID}), "self", "GET"),
                new HATEOASLinkResponseModel(urlHelper.Link("HATEOAS", new {controller = "products", id = ID}), "update_product", "PUT"),
                new HATEOASLinkResponseModel(urlHelper.Link("HATEOAS", new {controller = "products", id = ID}), "partially_update_product", "PATCH"),
                new HATEOASLinkResponseModel(urlHelper.Link("HATEOAS", new {controller = "products", id = ID}), "discontinue_or_delete_discontinued_product", "DELETE")
            };
        }
    }
}