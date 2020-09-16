using RestfulTestful.Models;
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

        public IHttpActionResult Get(int id)
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            if (db.Table<Product>().Where(c => c.ID.Equals(id)).Any())
            {
                return Ok<ProductResponseModel>(new ProductResponseModel(db.GetAllWithChildren<Product>().First(c => c.ID.Equals(id)), this.Url));
            }
            return NotFound();
        }

        public IHttpActionResult Post([FromBody]Product product)
        {
            if (product.Name != null && product.Category != null)
            {
                string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
                SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
                product.TokenNumber = 1;
                if (!POEChecker.AlreadyIsInDatabase(product))
                {
                    if (db.Insert(product) == 1)
                    {
                        return Ok<ProductResponseModel>(new ProductResponseModel(db.Table<Product>().Last(p => p.Name.Equals(product.Name) && p.Category.Equals(product.Category) && p.Price.Equals(product.Price) && p.Discount.Equals(product.Discount)), this.Url));
                    }
                    return InternalServerError(new Exception("Couldn't insert row into database"));
                }
                return BadRequest("Object already is in database!");
            }
            return BadRequest("Product name and category can't be null!.");
        }

        public IHttpActionResult Put(int id, [FromBody]Product newProduct)
        {
            if (newProduct.Name != null && newProduct.Category != null)
            {
                string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
                SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
                if (db.Table<Product>().Where(p => p.ID.Equals(id)).Any())
                {
                    Product oldProduct = db.Table<Product>().First(p => p.ID.Equals(id));
                    if (newProduct.TokenNumber.Equals(oldProduct))
                    {
                        newProduct.TokenNumber++;
                        newProduct.ID = id;
                        if (db.Update(newProduct) == 1)
                        {
                            return Ok<ProductResponseModel>(new ProductResponseModel(db.GetAllWithChildren<Product>().First(p => p.ID.Equals(id)), this.Url));
                        }
                        return InternalServerError(new Exception("Couldn't update row."));
                    }
                    return BadRequest("Wrong token value.");
                }
                return NotFound();
            }
            return BadRequest("Product name and category can't be null!.");
        }

        public IHttpActionResult Patch(int id, [FromBody]Delta<Product> delta)
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
                        delta.Patch(product);
                        product.ID = id;
                        product.TokenNumber++;
                        if (db.Update(product) == 1)
                        {
                            return Ok<ProductResponseModel>(new ProductResponseModel(product, this.Url));
                        }
                        return InternalServerError(new Exception("Couldn't update row."));
                    }
                    return BadRequest("Wrong token value.");
                }
                return InternalServerError(new Exception("Error during getting token value."));
            }
            return NotFound();
        }

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
                        if (product.Sales.Where(s => !s.Payed && !s.Archieved).Any())
                        {
                            return InternalServerError(new Exception("Cannot discontinue product with unpaid unarchieved sales."));
                        }
                        if (product.Discontinued)
                        {
                            if(db.Delete(product)!=1)
                            {
                                return InternalServerError(new Exception("Couldn't delete row."));
                            }
                            return Ok<Product>(product);
                        }
                        else
                        {
                            product.Discontinued = true;
                            product.TokenNumber++;
                            for (int i = 0; i < product.Sales.Count; i++)
                            {
                                product.Sales[i].Archieved = true;
                                product.Sales[i].TokenNumber++;
                            }
                            db.UpdateWithChildren(product);
                        }
                        return Ok<ProductResponseModel>(new ProductResponseModel(product, this.Url));
                    }
                    return BadRequest("Wrong token value.");
                }
                return InternalServerError(new Exception("Error during getting token value."));
            }
            return NotFound();
        }
    }
}
