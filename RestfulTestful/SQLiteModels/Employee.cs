using SQLite;

namespace RestfulTestful.SQLiteModels
{
    [Table("Employee")]
    public class Employee
    {
        [PrimaryKey, AutoIncrement]
        public long ID { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public long TokenNumber { get; set; }
        public bool Inactive { get; set; }
    }
}