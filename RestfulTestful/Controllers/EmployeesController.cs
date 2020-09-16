using RestfulTestful.Models;
using RestfulTestful.SQLiteModels;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.OData;

namespace RestfulTestful.Controllers
{
    [System.Web.Http.Authorize(Roles ="Admin")]
    public class EmployeesController : ApiController
    {
        [EnableQuery]
        public IHttpActionResult Get()
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            if (db.Table<Employee>().Any())
            {
                List<EmployeeResponseModel> employeeResponseModels = new List<EmployeeResponseModel>();
                List<Employee> employees = db.Table<Employee>().ToList();
                foreach(Employee e in employees)
                {
                    employeeResponseModels.Add(new EmployeeResponseModel(e, this.Url));
                }
                return Ok<IEnumerable<EmployeeResponseModel>>(employeeResponseModels);
            }
            return NotFound();
        }

        public IHttpActionResult Get(int id)
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            if (db.Table<Employee>().Where(c => c.ID.Equals(id)).Any())
            {
                return Ok<EmployeeResponseModel>(new EmployeeResponseModel(db.Table<Employee>().First(c => c.ID.Equals(id)), this.Url));
            }
            return NotFound();
        }

        public IHttpActionResult Post([FromBody]Employee employee)
        {
            if (employee.Name != null && employee.Name.Length > 0)
            {
                if (employee.Password != null && employee.Password.Length > 6 &&
                    Regex.IsMatch(employee.Password, @"[!,@,#,$,%,^,&,*,?,_,~,-,£,(,)]") &&
                    Regex.IsMatch(employee.Password, @"\d+") &&
                    Regex.IsMatch(employee.Password, @"[a-z]") &&
                    Regex.IsMatch(employee.Password, @"[A-Z]"))
                {
                    string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
                    SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
                    employee.TokenNumber = 1;
                    if (POEChecker.AlreadyIsInDatabase(employee))
                    {
                        if (db.Insert(employee) == 1)
                        {
                            return Ok<EmployeeResponseModel>(new EmployeeResponseModel(db.Table<Employee>().Last(e => e.Name.Equals(employee.Name) && e.Password.Equals(employee.Password)), this.Url));
                        }
                        return InternalServerError();
                    }
                    return BadRequest("Object already is in database!");
                }
                return BadRequest("Your password must be at least 6 characters long and must contain at least one special, at least one number, at least one small letter and at least one capital letter");
            }
            return BadRequest("Your nickname is too short");
        }

        public IHttpActionResult Put(int id, [FromBody]Employee newEmployee)
        {
            if (newEmployee != null && newEmployee.Name.Length > 0)
            {
                if (newEmployee.Password != null && newEmployee.Password.Length > 6 &&
                    Regex.IsMatch(newEmployee.Password, @".[!,@,#,$,%,^,&,*,?,_,~,-,£,(,)]") &&
                    Regex.IsMatch(newEmployee.Password, @"\d+") &&
                    Regex.IsMatch(newEmployee.Password, @"[a-z]") &&
                    Regex.IsMatch(newEmployee.Password, @"[A-Z]"))
                {
                    string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
                    SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
                    if (db.Table<Employee>().Where(e => e.ID.Equals(id)).Any())
                    {
                        Product oldProduct = db.Table<Product>().First(p => p.ID.Equals(id));
                        if (newEmployee.TokenNumber.Equals(oldProduct))
                        {
                            newEmployee.TokenNumber++;
                            newEmployee.ID = id;
                            if (db.Update(newEmployee) == 1)
                            {
                                return Ok<EmployeeResponseModel>(new EmployeeResponseModel(newEmployee, this.Url));
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
        public IHttpActionResult Patch(int id, [FromBody]Delta<Employee> delta)
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
                            if (db.Table<Employee>().Where(e => e.ID.Equals(id)).Any())
                            {
                                Employee employee = db.Table<Employee>().First(e => e.ID.Equals(id));
                                object requestToken = null;
                                if (delta.TryGetPropertyValue("TokenNumber", out requestToken))
                                {
                                    if (employee.TokenNumber.Equals((long)requestToken))
                                    {
                                        delta.Patch(employee);
                                        employee.ID = id;
                                        employee.TokenNumber++;
                                        if (db.Update(employee) == 1)
                                        {
                                            return Ok<EmployeeResponseModel>(new EmployeeResponseModel(employee, this.Url));
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
        public IHttpActionResult Delete(int id, [FromBody]Delta<Employee> delta)
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            if (db.Table<Employee>().Where(u => u.ID.Equals(id)).Any())
            {
                Employee employee = db.Table<Employee>().First(e => e.ID.Equals(id));
                object requestToken = null;
                if (delta.TryGetPropertyValue("TokenNumber", out requestToken))
                {
                    if (employee.TokenNumber.Equals((long)requestToken))
                    {
                        if (employee.Inactive)
                        {
                            if(db.Delete(employee)!=1)
                            {
                                return InternalServerError(new Exception("Couldn't delete row."));
                            }
                            return Ok<Employee>(employee);
                        }
                        else
                        {
                            employee.Inactive = true;
                            employee.TokenNumber++;
                            db.Update(employee);
                        }
                        return Ok<EmployeeResponseModel>(new EmployeeResponseModel(employee, this.Url));
                    }
                    return BadRequest("Wrong token value.");
                }
                return InternalServerError(new Exception("Error during getting token value."));
            }
            return NotFound();
        }
    }
}
