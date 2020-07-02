using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData;
using FromBodyAttribute = System.Web.Http.FromBodyAttribute;

namespace RestfulTestful.Controllers
{
    public class User
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public string LastName { get; set; }
        public long Age { get; set; }
        public double Price { get; set; }
    }
    public class ResponseUser
    {
        public bool Status { set; get; }
        public object Date { set; get; }
    }
    public class ValuesController : ApiController
    {
        static List<User> l = new List<User>();
        static int count = 0;
        // GET api/values
        [EnableQuery]
        public IHttpActionResult Get()
        {
            count++;
            l.Add(new User() { ID = count, UserName = "user"+count.ToString(), LastName = "lastName" + count.ToString(), Age = count, Price = count*0.2 });
            ResponseUser r = new ResponseUser() { Status = true, Date = l };
            return Ok<IEnumerable<User>>(l);
        }

        // GET api/values/5
        public User Get(int id)
        {
            return l.Where(u => u.ID==id).First();
        }
        public User Get(int id, string category)
        {
            count++;
            l.Add(new User() { ID = count, UserName = "userCAT" + count.ToString() });
            return l.Where(u => u.ID == id).First();
        }

        // POST api/values
        public void Post([FromBody] string value)
        {

        }

        // PUT api/values/5
        public void Put(int id, [FromBody] string value)
        {
        }
        public void Patch(int id, [FromBody] Delta<User> delta)
        {
            delta.Patch(l.First(u => u.ID == id));
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
