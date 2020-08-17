using Microsoft.AspNetCore.Authorization;
using RestfulTestful.SQLiteModels;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Security;

namespace RestfulTestful.Controllers
{
    [System.Web.Http.Authorize(Roles ="Admin")]
    public class RegistrationController : ApiController
    {
        // GET: api/Registration
        [EnableQuery]
        public IHttpActionResult Get()
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            if (db.Table<User>().Any())
            {
                return Ok<IEnumerable<User>>(db.Table<User>().ToList());
            }
            return NotFound();
        }

        // GET: api/Registration/5
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

        // POST: api/Registration
        public IHttpActionResult Post([FromBody]User user)
        {
            if (user.Name != null && user.Name.Length > 0)
            {
                if (user.Password != null && user.Password.Length > 6 &&
                    Regex.IsMatch(user.Password, @"[!,@,#,$,%,^,&,*,?,_,~,-,£,(,)]") &&
                    Regex.IsMatch(user.Password, @"\d+") &&
                    Regex.IsMatch(user.Password, @"[a-z]") &&
                    Regex.IsMatch(user.Password, @"[A-Z]"))
                {
                    string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
                    SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
                    user.TokenNumber = 1;
                    if (db.Insert(user) == 1)
                    {
                        return Ok<User>(db.Table<User>().Last(u => u.Name.Equals(user.Name) && u.Password.Equals(user.Password)));
                    }
                    return InternalServerError();
                }
                return BadRequest("Your password must be at least 6 characters long and must contain at least one special, at least one number, at least one small letter and at least one capital letter");
            }
            return BadRequest("Your nickname is too short");
        }

        // PUT: api/Registration/5
        public IHttpActionResult Put(int id, [FromBody]User newUser)
        {
            if (newUser != null && newUser.Name.Length > 0)
            {
                if (newUser.Password != null && newUser.Password.Length > 6 &&
                    Regex.IsMatch(newUser.Password, @".[!,@,#,$,%,^,&,*,?,_,~,-,£,(,)]") &&
                    Regex.IsMatch(newUser.Password, @"\d+") &&
                    Regex.IsMatch(newUser.Password, @"[a-z]") &&
                    Regex.IsMatch(newUser.Password, @"[A-Z]"))
                {
                    string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
                    SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
                    if (db.Table<User>().Where(u => u.ID.Equals(id)).Any())
                    {
                        Product oldProduct = db.Table<Product>().First(p => p.ID.Equals(id));
                        if (newUser.TokenNumber.Equals(oldProduct))
                        {
                            newUser.TokenNumber++;
                            if (db.Update(newUser) == 1)
                            {
                                return Ok<User>(newUser);
                            }
                            return InternalServerError(new Exception("Couldn't update row."));
                        }
                        return BadRequest("Wrong token value.");
                    }
                    return NotFound();
                }
                return BadRequest("Password must be at least 6 characters long and must contain at least one special character, at least one number, at least one small letter and at least one capital letter");
            }
            return BadRequest("Nickname is too short");
        }
        [System.Web.Http.Authorize(Roles = "Employee, Admin")]
        public IHttpActionResult Patch(int id, [FromBody]Delta<User> delta)
        {
            object name = null;
            if (delta.TryGetPropertyValue("Name", out name))
            {
                object password = null;
                if (delta.TryGetPropertyValue("Password", out password))
                {
                    if (name != null && name.ToString().Length > 0)
                    {
                        if (password != null && password.ToString().Length > 6 &&
                            Regex.IsMatch(password.ToString(), @".[!,@,#,$,%,^,&,*,?,_,~,-,£,(,)]") &&
                            Regex.IsMatch(password.ToString(), @"\d+") &&
                            Regex.IsMatch(password.ToString(), @"[a-z]") &&
                            Regex.IsMatch(password.ToString(), @"[A-Z]"))
                        {
                            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
                            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
                            if (db.Table<User>().Where(p => p.ID.Equals(id)).Any())
                            {
                                User user = db.Table<User>().First(u => u.ID.Equals(id));
                                object requestToken = null;
                                if (delta.TryGetPropertyValue("TokenNumber", out requestToken))
                                {
                                    if (user.TokenNumber.Equals((long)requestToken))
                                    {
                                        delta.Patch(user);
                                        user.TokenNumber++;
                                        if (db.Update(user) == 1)
                                        {
                                            return Ok<User>(user);
                                        }
                                        return InternalServerError(new Exception("Couldn't update row."));
                                    }
                                    return BadRequest("Wrong token value.");
                                }
                                return InternalServerError(new Exception("Error during getting token value."));
                            }
                            return NotFound();
                        }
                        return BadRequest("Password must be at least 6 characters long and must contain at least one special character, at least one number, at least one small letter and at least one capital letter");
                    }
                    return BadRequest("Nickname is too short");
                }
                return InternalServerError(new Exception("Error during getting password value."));
            }
            return InternalServerError(new Exception("Error during getting nickname value."));
        }

        // DELETE: api/Registration/5
        public IHttpActionResult Delete(int id, [FromBody]Delta<User> delta)
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            if (db.Table<User>().Where(u => u.ID.Equals(id)).Any())
            {
                User user = db.Table<User>().First(p => p.ID.Equals(id));
                object requestToken = null;
                if (delta.TryGetPropertyValue("TokenNumber", out requestToken))
                {
                    if (user.TokenNumber.Equals((long)requestToken))
                    {
                        user.Inactive = true;
                        user.TokenNumber++;
                        db.Update(user);
                        return Ok<User>(user);
                    }
                    return BadRequest("Wrong token value.");
                }
                return InternalServerError(new Exception("Error during getting token value."));
            }
            return NotFound();
        }
    }
}
