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
    public class SalesController : ApiController
    {
        [EnableQuery]
        public IHttpActionResult Get()
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            if (db.Table<Sale>().Any())
            {
                List<SaleResponseModel> saleResponseModels = new List<SaleResponseModel>();
                List<Sale> sales = db.GetAllWithChildren<Sale>();
                foreach(Sale s in sales)
                {
                    saleResponseModels.Add(new SaleResponseModel(s, this.Url, false));
                }
                return Ok<IEnumerable<SaleResponseModel>>(saleResponseModels);
            }
            return NotFound();
        }

        [AllowAnonymous]
        public IHttpActionResult Get(int id)
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            if (db.Table<Sale>().Where(c => c.ID.Equals(id)).Any())
            {
                return Ok<SaleResponseModel>(new SaleResponseModel(db.GetAllWithChildren<Sale>().First(c => c.ID.Equals(id)), this.Url, true));
            }
            return NotFound();
        }

        public IHttpActionResult Post([FromBody]Sale sale)
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            sale.TokenNumber = 1;
            DateTime now = DateTime.Now;
            sale.AddDate = now;
            if (!POEChecker.AlreadyIsInDatabase(sale))
            {
                if (db.Table<Product>().Where(p => p.ID.Equals(sale.ProductID)).Any() && db.Table<Client>().Where(c => c.ID.Equals(sale.ClientID)).Any())
                {
                    Product product = db.Table<Product>().Where(p => p.ID.Equals(sale.ProductID)).First();
                    if (product.Quantity > sale.Quantity && !product.Discontinued)
                    {
                        product.Quantity = product.Quantity - sale.Quantity;
                        product.TokenNumber++;
                        if (db.Update(product) == 1 && db.Insert(sale) == 1)
                        {
                            return Ok<SaleResponseModel>(new SaleResponseModel(db.GetAllWithChildren<Sale>().Last(s => s.ProductID.Equals(sale.ProductID) && s.ClientID.Equals(sale.ClientID) && s.AddDate.Equals(now)), this.Url, true));
                        }
                        return InternalServerError(new Exception("Couldn't insert row into database"));
                    }
                    return BadRequest("Insufficient product quantity or product discontinued");
                }
                return BadRequest("Invalid product or/and client ID");
            }
            return BadRequest("Object already is in database!");
        }

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
                        newSale.ID = id;
                        newSale.TokenNumber++;
                        if (db.Update(newSale) == 1)
                        {
                            return Ok<SaleResponseModel>(new SaleResponseModel(db.GetAllWithChildren<Sale>().First(s => s.ID.Equals(id)), this.Url, true));
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
                            if (sale.Quantity.Equals((long)newQuantity) || (long)newQuantity == 0)
                            {
                                delta.Patch(sale);
                                sale.ID = id;
                                sale.TokenNumber++;
                                if (db.Update(sale) == 1)
                                {
                                    return Ok<SaleResponseModel>(new SaleResponseModel(sale, this.Url, true));
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
                            return Ok<Sale>(sale);
                        }
                        else
                        {
                            if (!sale.Payed)
                            {
                                Product product = db.Table<Product>().Where(p => p.ID.Equals(sale.ProductID)).First();
                                product.Quantity = product.Quantity + sale.Quantity;
                                product.TokenNumber++;
                                if(db.Update(product)!=1)
                                {
                                    return InternalServerError(new Exception("Couldn't update product quantity with amount from unpaid sale."));
                                }
                            }
                            sale.TokenNumber++;
                            sale.Archieved = true;
                            if(db.Update(sale)!=1)
                            {
                                return InternalServerError(new Exception("Couldn't update sale."));
                            }
                        }
                        return Ok<SaleResponseModel>(new SaleResponseModel(sale, this.Url, true));
                    }
                    return BadRequest("Wrong token value.");
                }
                return InternalServerError(new Exception("Error during getting token value."));
            }
            return NotFound();
        }
        private List<HATEOASLinkResponseModel> ProduceLinks(long saleID)
        {
            return new List<HATEOASLinkResponseModel>()
            {
                new HATEOASLinkResponseModel(this.Url.Link("HATEOAS", new {id = saleID}), "self", "GET"),
                new HATEOASLinkResponseModel(this.Url.Link("HATEOAS", new {id = saleID}), "update_client", "PUT"),
                new HATEOASLinkResponseModel(this.Url.Link("HATEOAS", new {id = saleID}), "partially_update_client", "PATCH"),
                new HATEOASLinkResponseModel(this.Url.Link("HATEOAS", new {id = saleID}), "archieve_or_delete_archieved_client", "DELETE")
            };
        }
    }
}
