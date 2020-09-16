using RestfulTestful.Models;
using RestfulTestful.SQLiteModels;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.OData;

namespace RestfulTestful.Controllers
{
    [Authorize]
    public class ClientsController : ApiController
    {
        [EnableQuery]
        public IHttpActionResult Get()
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            if (db.Table<Client>().Any())
            {
                List<ClientResponseModel> clientResponseModels = new List<ClientResponseModel>();
                List<Client> clients = db.Table<Client>().ToList();
                foreach(Client c in clients)
                {
                    clientResponseModels.Add(new ClientResponseModel(c, this.Url));
                }
                return Ok<IEnumerable<ClientResponseModel>>(clientResponseModels);
            }
            return NotFound();
        }

        public IHttpActionResult Get(int id)
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            if(db.Table<Client>().Where(c => c.ID.Equals(id)).Any())
            {
                return Ok<ClientResponseModel>(new ClientResponseModel(db.GetAllWithChildren<Client>().First(c => c.ID.Equals(id)), this.Url));
            }
            return NotFound();
        }

        public IHttpActionResult Post([FromBody]Client client)
        {
            if (client.Name != null && client.Surname != null && client.Address != null)
            {
                string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
                SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
                client.TokenNumber = 1;
                if (!POEChecker.AlreadyIsInDatabase(client))
                {
                    if (db.Insert(client) == 1)
                    {
                        ClientResponseModel clientResponseModel = new ClientResponseModel(db.Table<Client>().Last(c => c.Name.Equals(client.Name) && c.Surname.Equals(client.Surname) && c.PhoneNumber.Equals(client.PhoneNumber) && c.Address.Equals(client.Address)), this.Url);
                        return Ok<ClientResponseModel>(clientResponseModel);
                    }
                    return InternalServerError(new Exception("Couldn't insert row into database"));
                }
                return BadRequest("Object already is in database!");
            }
            return BadRequest("Client name, surname and address can't be null");
        }

        public IHttpActionResult Put(int id, [FromBody]Client newClient)
        {
            if (newClient.Name != null && newClient.Surname != null && newClient.Address != null)
            {
                string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
                SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
                if (db.Table<Client>().Where(c => c.ID.Equals(id)).Any())
                {
                    Client oldClient = db.Table<Client>().First(c => c.ID.Equals(id));
                    if (newClient.TokenNumber.Equals(oldClient.TokenNumber))
                    {
                        newClient.TokenNumber++;
                        newClient.ID = id;
                        if (db.Update(newClient) == 1)
                        {
                            return Ok<ClientResponseModel>(new ClientResponseModel(db.GetAllWithChildren<Client>().First(c => c.ID.Equals(id)), this.Url));
                        }
                        return InternalServerError(new Exception("Couldn't update row."));
                    }
                    return BadRequest("Wrong token value.");
                }
                return NotFound();
            }
            return BadRequest("Client name, surname and address can't be null");
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
                        client.ID = id;
                        client.TokenNumber++;
                        if (db.Update(client) == 1)
                        {
                            return Ok<ClientResponseModel>(new ClientResponseModel(client, this.Url));
                        }
                        return InternalServerError(new Exception("Couldn't update row."));
                    }
                    return BadRequest("Wrong token value.");
                }
                return InternalServerError(new Exception("Error during getting token value."));
            }
            return NotFound();
        }

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
                        if (client.Sales.Where(s => !s.Payed && !s.Archieved).Any())
                        {
                            return InternalServerError(new Exception("Cannot delete/archieve client with unpaid unarchieved sales."));
                        }
                        if (client.Deleted)
                        {
                            if(db.Delete(client)!=1)
                            {
                                return InternalServerError(new Exception("Couldn't delete row."));
                            }
                            return Ok<Client>(client);
                        }
                        else
                        {
                            client.Deleted = true;
                            client.TokenNumber++;
                            for (int i = 0; i < client.Sales.Count; i++)
                            {
                                client.Sales[i].Archieved = true;
                                client.Sales[i].TokenNumber++;
                            }
                            db.UpdateWithChildren(client);
                        }
                        return Ok<ClientResponseModel>(new ClientResponseModel(client, this.Url));
                    }
                    return BadRequest("Wrong token value.");
                }
                return InternalServerError(new Exception("Error during getting token value."));
            }
            return NotFound();
        }
    }
}
