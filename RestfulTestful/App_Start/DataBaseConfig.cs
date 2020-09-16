using RestfulTestful.SQLiteModels;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace RestfulTestful.App_Start
{
    public class DataBaseConfig
    {
        public static void SetUpDatabaseIfItDoesntExist()
        {
            DoHack();
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RestfulTestfulFiles");
            Directory.CreateDirectory(path);
            SQLiteConnection db = new SQLiteConnection(System.IO.Path.Combine(path, "RestfulTestfulDatabase.db"));
            db.CreateTable<Product>();
            db.CreateTable<Client>();
            db.CreateTable<Sale>();
            db.CreateTable<Employee>();
            //db.Insert(new Employee() { Name="admin", Password="admin1", Inactive=false});
        }
        public static bool Hacked { get; private set; }

        public static bool DoHack()
        {
            if (Hacked) return true;

            try
            {
                const string runtimeFolderName = "/runtimes";

                var destinationPath = typeof(SQLitePCL.raw).Assembly.Location.Replace("\\", "/");
                var destinationLength = destinationPath.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
                var destinationDirectory = destinationPath.Substring(0, destinationLength) + runtimeFolderName;

                var sourcePath = new Uri(typeof(SQLitePCL.raw).Assembly.CodeBase).AbsolutePath;
                var sourceLength = sourcePath.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
                var sourceDirectory = sourcePath.Substring(0, sourceLength) + runtimeFolderName;

                if (Directory.Exists(sourceDirectory))CopyFilesRecursively(new DirectoryInfo(sourceDirectory), new DirectoryInfo(destinationDirectory));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }

            return (Hacked = true);
        }

        private static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (var dir in source.GetDirectories())
            {
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            }

            foreach (var file in source.GetFiles())
            {
                try
                {
                    var destinationFile = Path.Combine(target.FullName, file.Name);
                    if (!File.Exists(destinationFile))
                        file.CopyTo(destinationFile);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }
    }
}