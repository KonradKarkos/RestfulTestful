using RestfulTestful.SQLiteModels;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Linq;
using System.Web.Http;

namespace RestfulTestful.Controllers
{
    public class MergeClientsController : ApiController
    {
        public IHttpActionResult Post(int id, int clientToAbsorbID)
        {
            bool roolback = false;
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            if(db.Table<Client>().Where(c => c.ID == id).Any())
            {
                if (db.Table<Client>().Where(c => c.ID == clientToAbsorbID).Any())
                {
                    Client clientToAbsrob = db.GetAllWithChildren<Client>().Where(c => c.ID == clientToAbsorbID).First();
                    for(int i=0; i< clientToAbsrob.Sales.Count; i++)
                    {
                        clientToAbsrob.Sales[i].ClientID = id;
                        if(db.Update(clientToAbsrob.Sales[i]) != 1)
                        {
                            roolback = true;
                            break;
                        }
                    }
                    if(roolback || db.Delete(clientToAbsrob) != 1)
                    {
                        for (int i = 0; i < clientToAbsrob.Sales.Count; i++)
                        {
                            clientToAbsrob.Sales[i].ClientID = clientToAbsorbID;
                            while(db.Update(clientToAbsrob.Sales[i]) != 1) { }
                        }
                        return InternalServerError(new Exception("Couldn't perform all operations. Every change has been rollbacked."));
                    }
                    return Ok<Client>(db.GetAllWithChildren<Client>(c => c.ID.Equals(id)).First());
                }
                return BadRequest("Couldn't find client to merge with given ID");
            }
            return BadRequest("Couldn't find client with given ID");
        }
    }
}
