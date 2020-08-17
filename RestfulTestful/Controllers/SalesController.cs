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
    public class SalesController : ApiController
    {
        // GET: api/Sales
        [EnableQuery]
        public IHttpActionResult Get()
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            if (db.Table<Sale>().Any())
            {
                return Ok<IEnumerable<Sale>>(db.GetAllWithChildren<Sale>());
            }
            return NotFound();
        }

        // GET: api/Sales/5
        [AllowAnonymous]
        public IHttpActionResult Get(int id)
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            if (db.Table<Sale>().Where(c => c.ID.Equals(id)).Any())
            {
                return Ok<Sale>(db.Table<Sale>().First(c => c.ID.Equals(id)));
            }
            return NotFound();
        }

        // POST: api/Sales
        public IHttpActionResult Post([FromBody]Sale sale)
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            sale.TokenNumber = 1;
            DateTime now = DateTime.Now;
            sale.AddDate = now;
            if (db.Table<Product>().Where(p => p.ID.Equals(sale.ProductID)).Any())
            {
                Product product = db.Table<Product>().Where(p => p.ID.Equals(sale.ProductID)).First();
                if (product.Quantity < sale.Quantity || product.Discontinued)
                {
                    product.Quantity = product.Quantity - sale.Quantity;
                    product.TokenNumber++;
                    if (db.Update(product) == 1 && db.Insert(sale) == 1)
                    {
                        return Ok<Sale>(db.GetAllWithChildren<Sale>().Last(s => s.ProductID.Equals(sale.ProductID) && s.ClientID.Equals(sale.ClientID) && s.AddDate.Equals(now)));
                    }
                    return InternalServerError();
                }
                return BadRequest("Insufficient product quantity or product discontinued");
            }
            return BadRequest("Invalid product ID");
        }

        // PUT: api/Sales/5
        public IHttpActionResult Put(int id, [FromBody]Sale newSale)
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            if (db.Table<Sale>().Where(p => p.ID.Equals(id)).Any())
            {
                Sale oldSale = db.GetAllWithChildren<Sale>().First(s => s.ID.Equals(id));
                if (newSale.TokenNumber.Equals(oldSale.TokenNumber))
                {
                    if (newSale.Quantity.Equals(oldSale.Quantity))
                    {
                        newSale.TokenNumber++;
                        if (db.Update(newSale) == 1)
                        {
                            return Ok<Sale>(newSale);
                        }
                        return InternalServerError(new Exception("Couldn't update row."));
                    }
                    return BadRequest("Can't change sale quantity.");
                }
                return BadRequest("Wrong token value.");
            }
            return NotFound();
        }

        public IHttpActionResult Patch(int id, [FromBody]Delta<Sale> delta)
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            if (db.Table<Sale>().Where(s => s.ID.Equals(id)).Any())
            {
                Sale sale = db.GetAllWithChildren<Sale>().First(s => s.ID.Equals(id));
                object requestToken = null;
                if (delta.TryGetPropertyValue("TokenNumber", out requestToken))
                {
                    if (sale.TokenNumber.Equals((long)requestToken))
                    {
                        object newQuantity = null;
                        if (delta.TryGetPropertyValue("Quantity", out newQuantity))
                        {
                            if (sale.Quantity.Equals((long)newQuantity) || newQuantity == null)
                            {
                                delta.Patch(sale);
                                sale.TokenNumber++;
                                if (db.Update(sale) == 1)
                                {
                                    return Ok<Sale>(sale);
                                }
                                return InternalServerError(new Exception("Couldn't update row."));
                            }
                            return BadRequest("Sale quantity is different or it wasn't provided.");
                        }
                        return InternalServerError(new Exception("Error during getting quantity value."));
                    }
                    return BadRequest("Wrong token value.");
                }
                return InternalServerError(new Exception("Error during getting token value."));
            }
            return NotFound();
        }

        // DELETE: api/Sales/5
        public IHttpActionResult Delete(int id, [FromBody]Delta<Sale> delta)
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            if (db.Table<Sale>().Where(s => s.ID.Equals(id)).Any())
            {
                Sale sale = db.GetAllWithChildren<Sale>().First(s => s.ID.Equals(id));
                object requestToken = null;
                if (delta.TryGetPropertyValue("TokenNumber", out requestToken))
                {
                    if (sale.TokenNumber.Equals((long)requestToken))
                    {
                        if (sale.Archieved)
                        {
                            if(db.Delete(sale)!=1)
                            {
                                return InternalServerError(new Exception("Couldn't delete row."));
                            }
                        }
                        else
                        {
                            if (sale.Payed)
                            {
                                Product product = db.Table<Product>().Where(p => p.ID.Equals(sale.ProductID)).First();
                                product.Quantity = product.Quantity + sale.Quantity;
                                product.TokenNumber++;
                                if(db.Update(product)!=1)
                                {
                                    return InternalServerError(new Exception("Couldn't update product after archievieng unpaid sale."));
                                }
                            }
                            sale.TokenNumber++;
                            sale.Archieved = true;
                            if(db.Update(sale)!=1)
                            {
                                return InternalServerError(new Exception("Couldn't update sale."));
                            }
                        }
                        return Ok<Sale>(sale);
                    }
                    return BadRequest("Wrong token value.");
                }
                return InternalServerError(new Exception("Error during getting token value."));
            }
            return NotFound();
        }
    }
}
