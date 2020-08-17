using Microsoft.Ajax.Utilities;
using RestfulTestful.SQLiteModels;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData;

namespace RestfulTestful.Controllers
{
    [Authorize]
    public class ClientsController : ApiController
    {
        // GET: api/Clients
        [EnableQuery]
        public IHttpActionResult Get()
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            if (db.Table<Client>().Any())
            {
                return Ok<IEnumerable<Client>>(db.Table<Client>().ToList());
            }
            return NotFound();
        }

        // GET: api/Clients/5
        public IHttpActionResult Get(int id)
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            if(db.Table<Client>().Where(c => c.ID.Equals(id)).Any())
            {
                return Ok<Client>(db.GetAllWithChildren<Client>().First(c => c.ID.Equals(id)));
            }
            return NotFound();
        }

        // POST: api/Clients
        public IHttpActionResult Post([FromBody]Client client)
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            client.TokenNumber = 1;
            if(db.Insert(client) == 1)
            {
                return Ok<Client>(db.GetAllWithChildren<Client>().Last(c => c.Name.Equals(client.Name) && c.Surname.Equals(client.Surname)));
            }
            return InternalServerError();
        }

        // PUT: api/Clients/5
        public IHttpActionResult Put(int id, [FromBody]Delta<Client> delta)
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            if (db.Table<Client>().Where(c => c.ID.Equals(id)).Any())
            {
                Client client = db.GetAllWithChildren<Client>().First(c => c.ID.Equals(id));
                object requestToken = null;
                if (delta.TryGetPropertyValue("TokenNumber", out requestToken))
                {
                    if (client.TokenNumber.Equals((long)requestToken))
                    {
                        client.TokenNumber++;
                        if (db.Update(client) == 1)
                        {
                            return Ok<Client>(client);
                        }
                        return InternalServerError(new Exception("Couldn't update row."));
                    }
                    return BadRequest("Wrong token value.");
                }
                return InternalServerError(new Exception("Error during getting token value."));
            }
            return NotFound();
        }
        public IHttpActionResult Patch(int id, [FromBody]Delta<Client> delta)
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            if (db.Table<Client>().Where(c => c.ID.Equals(id)).Any())
            {
                Client client = db.GetAllWithChildren<Client>().First(c => c.ID.Equals(id));
                object requestToken = null;
                if (delta.TryGetPropertyValue("TokenNumber", out requestToken))
                {
                    if (client.TokenNumber.Equals((long)requestToken))
                    {
                        delta.Patch(client);
                        client.TokenNumber++;
                        if (db.Update(client) == 1)
                        {
                            return Ok<Client>(client);
                        }
                        return InternalServerError(new Exception("Couldn't update row."));
                    }
                    return BadRequest("Wrong token value.");
                }
                return InternalServerError(new Exception("Error during getting token value."));
            }
            return NotFound();
        }

        // DELETE: api/Clients/5
        public IHttpActionResult Delete(int id, [FromBody]Delta<Client> delta)
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            if (db.Table<Client>().Where(c => c.ID.Equals(id)).Any())
            {
                Client client = db.GetAllWithChildren<Client>().First(c => c.ID.Equals(id));
                object requestToken = null;
                if (delta.TryGetPropertyValue("TokenNumber", out requestToken))
                {
                    if (client.TokenNumber.Equals((long)requestToken))
                    {
                        foreach (Sale s in client.Sales)
                        {
                            if (!s.Payed && !s.Archieved)
                            {
                                return InternalServerError(new Exception("Cannot delete client with unpaid sales."));
                            }
                        }
                        client.Deleted = true;
                        client.TokenNumber++;
                        for (int i = 0; i < client.Sales.Count; i++)
                        {
                            client.Sales[i].Archieved = true;
                            client.Sales[i].TokenNumber++;
                        }
                        db.UpdateWithChildren(client);
                        return Ok<Client>(client);
                    }
                    return BadRequest("Wrong token value.");
                }
                return InternalServerError(new Exception("Error during getting token value."));
            }
            return NotFound();
        }
    }
}
