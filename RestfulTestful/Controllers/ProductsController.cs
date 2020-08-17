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
    public class ProductsController : ApiController
    {
        // GET: api/Products
        [AllowAnonymous]
        [EnableQuery]
        public IHttpActionResult Get()
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            if (db.Table<Product>().Any())
            {
                return Ok<IEnumerable<Product>>(db.Table<Product>().ToList());
            }
            return NotFound();
        }

        // GET: api/Products/5
        [AllowAnonymous]
        public IHttpActionResult Get(int id)
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            if (db.Table<Product>().Where(c => c.ID.Equals(id)).Any())
            {
                return Ok<Product>(db.Table<Product>().First(c => c.ID.Equals(id)));
            }
            return NotFound();
        }

        // POST: api/Products
        public IHttpActionResult Post([FromBody]Product product)
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            product.TokenNumber = 1;
            if (db.Insert(product) == 1)
            {
                return Ok<Product>(db.Table<Product>().Last(p => p.Name.Equals(product.Name) && p.Category.Equals(product.Category)));
            }
            return InternalServerError();
        }

        // PUT: api/Products/5
        public IHttpActionResult Put(int id, [FromBody]Product newProduct)
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            if (db.Table<Product>().Where(p => p.ID.Equals(id)).Any())
            {
                Product oldProduct = db.Table<Product>().First(p => p.ID.Equals(id));
                if (newProduct.TokenNumber.Equals(oldProduct))
                {
                    newProduct.TokenNumber++;
                    if (db.Update(newProduct) == 1)
                    {
                        return Ok<Product>(newProduct);
                    }
                    return InternalServerError(new Exception("Couldn't update row."));
                }
                return BadRequest("Wrong token value.");
            }
            return NotFound();
        }

        public IHttpActionResult Patch(int id, [FromBody]Delta<Product> delta)
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            if (db.Table<Product>().Where(p => p.ID.Equals(id)).Any())
            {
                Product product = db.Table<Product>().First(p => p.ID.Equals(id));
                object requestToken = null;
                if (delta.TryGetPropertyValue("TokenNumber", out requestToken))
                {
                    if (product.TokenNumber.Equals((long)requestToken))
                    {
                        delta.Patch(product);
                        product.TokenNumber++;
                        if (db.Update(product) == 1)
                        {
                            return Ok<Product>(product);
                        }
                        return InternalServerError(new Exception("Couldn't update row."));
                    }
                    return BadRequest("Wrong token value.");
                }
                return InternalServerError(new Exception("Error during getting token value."));
            }
            return NotFound();
        }

        // DELETE: api/Products/5
        public IHttpActionResult Delete(int id, [FromBody]Delta<Product> delta)
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            if (db.Table<Product>().Where(p => p.ID.Equals(id)).Any())
            {
                Product product = db.GetAllWithChildren<Product>().First(p => p.ID.Equals(id));
                object requestToken = null;
                if (delta.TryGetPropertyValue("TokenNumber", out requestToken))
                {
                    if (product.TokenNumber.Equals((long)requestToken))
                    {
                        foreach (Sale s in product.Sales)
                        {
                            if (!s.Payed)
                            {
                                return InternalServerError(new Exception("Cannot discontinue product with unpaid sales."));
                            }
                        }
                        product.Discontinued = true;
                        product.TokenNumber++;
                        for (int i = 0; i < product.Sales.Count; i++)
                        {
                            product.Sales[i].Archieved = true;
                            product.Sales[i].TokenNumber++;
                        }
                        db.UpdateWithChildren(product);
                        return Ok<Product>(product);
                    }
                    return BadRequest("Wrong token value.");
                }
                return InternalServerError(new Exception("Error during getting token value."));
            }
            return NotFound();
        }
    }
}
