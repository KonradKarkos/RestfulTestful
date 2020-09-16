using RestfulTestful.SQLiteModels;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Routing;

namespace RestfulTestful.Models
{
    public class ClientResponseModel
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public long PhoneNumber { get; set; }
        public string Address { get; set; }
        public bool Deleted { get; set; }
        public long TokenNumber { get; set; }
        public List<Sale> Sales { get; set; }
        public List<HATEOASLinkResponseModel> Links { get; set; }
        public ClientResponseModel(Client client)
        {
            ID = client.ID;
            Name = client.Name;
            Surname = client.Surname;
            PhoneNumber = client.PhoneNumber;
            Address = client.Address;
            Deleted = client.Deleted;
            TokenNumber = client.TokenNumber;
            Sales = client.Sales;
            if (Sales != null)
            {
                Sales.ForEach(s => s.Client = null);
            }
        }
        public ClientResponseModel(Client client, UrlHelper urlHelper)
        {
            ID = client.ID;
            Name = client.Name;
            Surname = client.Surname;
            PhoneNumber = client.PhoneNumber;
            Address = client.Address;
            Deleted = client.Deleted;
            TokenNumber = client.TokenNumber;
            Sales = client.Sales;
            if (Sales != null)
            {
                Sales.ForEach(s => s.Client = null);
            }
            Links = new List<HATEOASLinkResponseModel>()
            {
                new HATEOASLinkResponseModel(urlHelper.Link("HATEOAS", new {controller = "clients", id = ID}), "self", "GET"),
                new HATEOASLinkResponseModel(urlHelper.Link("HATEOAS", new {controller = "clients", id = ID}), "update_client", "PUT"),
                new HATEOASLinkResponseModel(urlHelper.Link("HATEOAS", new {controller = "clients", id = ID}), "partially_update_client", "PATCH"),
                new HATEOASLinkResponseModel(urlHelper.Link("HATEOAS", new {controller = "clients", id = ID}), "archieve_or_delete_archieved_client", "DELETE")
            };
        }
    }
}