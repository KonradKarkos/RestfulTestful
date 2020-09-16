using RestfulTestful.SQLiteModels;
using System.Collections.Generic;
using System.Web.Http.Routing;

namespace RestfulTestful.Models
{
    public class EmployeeResponseModel
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public long TokenNumber { get; set; }
        public bool Inactive { get; set; }
        public List<HATEOASLinkResponseModel> Links { get; set; }
        public EmployeeResponseModel(Employee employee, UrlHelper urlHelper)
        {
            ID = employee.ID;
            Name = employee.Name;
            Password = employee.Password;
            Role = employee.Role;
            TokenNumber = employee.TokenNumber;
            Inactive = employee.Inactive;
            Links = new List<HATEOASLinkResponseModel>()
            {
                new HATEOASLinkResponseModel(urlHelper.Link("HATEOAS", new {controller = "employees", id = ID}), "self", "GET"),
                new HATEOASLinkResponseModel(urlHelper.Link("HATEOAS", new {controller = "employees", id = ID}), "update_employee", "PUT"),
                new HATEOASLinkResponseModel(urlHelper.Link("HATEOAS", new {controller = "employees", id = ID}), "partially_update_employee", "PATCH"),
                new HATEOASLinkResponseModel(urlHelper.Link("HATEOAS", new {controller = "employees", id = ID}), "deactivate_or_delete_deactivated_employee", "DELETE")
            };
        }
    }
}