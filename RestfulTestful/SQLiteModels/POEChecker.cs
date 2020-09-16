using SQLite;
using System.Linq;

namespace RestfulTestful.SQLiteModels
{
    public static class POEChecker
    {
        public static bool AlreadyIsInDatabase(Client client)
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            return db.Table<Client>().Where(c => c.Name.Equals(client.Name) && c.Surname.Equals(client.Surname) && c.PhoneNumber.Equals(client.PhoneNumber) && c.Address.Equals(client.Address)).Any();
        }
        public static bool AlreadyIsInDatabase(Product product)
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            return db.Table<Product>().Where(p => p.Name.Equals(product.Name) && p.Category.Equals(product.Category) && p.Price.Equals(product.Price) && p.Discount.Equals(product.Discount)).Any();
        }
        public static bool AlreadyIsInDatabase(Sale sale)
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            return db.Table<Sale>().Where(s => s.ProductID.Equals(sale.ProductID) && s.ClientID.Equals(sale.ClientID) && s.Quantity.Equals(sale.Quantity) && s.Payed.Equals(sale.Payed) && s.Archieved.Equals(sale.Archieved) && s.SaleDate.Equals(sale.SaleDate)).Any();
        }
        public static bool AlreadyIsInDatabase(Employee employee)
        {
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            return db.Table<Employee>().Where(e => e.Name.Equals(employee.Name) && e.Password.Equals(employee.Password) && e.Role.Equals(employee.Role)).Any();
        }
    }
}