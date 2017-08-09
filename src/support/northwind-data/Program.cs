using System;
using System.IO;
using Funq;
using ServiceStack;
using ServiceStack.Redis;
using ServiceStack.OrmLite;

namespace NorthwindData
{
    public class Program
    {
        static string[] Providers = new[]{ "sqlite", "sqlserver", "mysql", "postgres", "redis" };

        public static OrmLiteConnectionFactory GetDbFactory(string dbProvider, string connectionString)
        {
            if (dbProvider == null || connectionString == null)
                return null;

            switch (dbProvider.ToLower())
            {
                case "sqlite":
                    var filePath = connectionString.MapProjectPath();
                    if (!File.Exists(filePath))
                        throw new FileNotFoundException($"SQLite database not found at '{filePath}'");
                    return new OrmLiteConnectionFactory(connectionString, SqliteDialect.Provider);
                case "sqlserver":
                    return new OrmLiteConnectionFactory(connectionString, SqlServerDialect.Provider);
                case "sqlserver2012":
                    return new OrmLiteConnectionFactory(connectionString, SqlServer2012Dialect.Provider);
                case "sqlserver2014":
                    return new OrmLiteConnectionFactory(connectionString, SqlServer2014Dialect.Provider);
                case "sqlserver2016":
                    return new OrmLiteConnectionFactory(connectionString, SqlServer2016Dialect.Provider);
                case "sqlserver2017":
                    return new OrmLiteConnectionFactory(connectionString, SqlServer2017Dialect.Provider);
                case "mysql":
                    return new OrmLiteConnectionFactory(connectionString, MySqlDialect.Provider);
                case "postgres":
                case "postgresql":
                    return new OrmLiteConnectionFactory(connectionString, PostgreSqlDialect.Provider);
            }

            return null;
        }

        public static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                if (args.Length > 0)
                    Console.WriteLine("Invalid Arguments: " + args.Join(" "));

                Console.WriteLine("Syntax:");
                Console.WriteLine("northwind-data [provider] [connectionString]");

                Console.WriteLine("Available Providers:");
                Console.WriteLine(Providers.Join(", "));
                return;
            }

            var dbFactory = new OrmLiteConnectionFactory("~/../../demos/northwind.sqlite".MapProjectPath(), SqliteDialect.Provider);
            var db = dbFactory.Open();
            var categories = db.Select<Category>();
            var customers = db.Select<Customer>();
            var employees = db.Select<Employee>();
            var employeeTerritories = db.Select<EmployeeTerritory>();
            var orders = db.Select<Order>();
            var orderDetails = db.Select<OrderDetail>();
            var products = db.Select<Product>();
            var regions = db.Select<Region>();
            var shippers = db.Select<Shipper>();
            var suppliers = db.Select<Supplier>();
            var territories = db.Select<Territory>();
            db.Dispose();

            var provider = args[0];
            var connectionString = args[1];

            dbFactory = GetDbFactory(provider, connectionString);
            if (dbFactory != null)
            {
                using (db = dbFactory.Open())
                {
                    db.DropAndCreateTable<Category>();
                    db.DropAndCreateTable<Customer>();
                    db.DropAndCreateTable<Employee>();
                    db.DropAndCreateTable<EmployeeTerritory>();
                    db.DropAndCreateTable<Order>();
                    db.DropAndCreateTable<OrderDetail>();
                    db.DropAndCreateTable<Product>();
                    db.DropAndCreateTable<Region>();
                    db.DropAndCreateTable<Shipper>();
                    db.DropAndCreateTable<Supplier>();
                    db.DropAndCreateTable<Territory>();

                    db.InsertAll(categories);
                    db.InsertAll(customers);
                    db.InsertAll(employees);
                    db.InsertAll(employeeTerritories);
                    db.InsertAll(orders);
                    db.InsertAll(orderDetails);
                    db.InsertAll(products);
                    db.InsertAll(regions);
                    db.InsertAll(shippers);
                    db.InsertAll(suppliers);
                    db.InsertAll(territories);
                }
            }
            else if (provider == "redis")
            {
                var redisManager = new RedisManagerPool(connectionString);
                using (var redis = redisManager.GetClient())
                {
                    redis.StoreAll(categories);
                    redis.StoreAll(customers);
                    redis.StoreAll(employees);
                    redis.StoreAll(employeeTerritories);
                    redis.StoreAll(orders);
                    redis.StoreAll(orderDetails);
                    redis.StoreAll(products);
                    redis.StoreAll(regions);
                    redis.StoreAll(shippers);
                    redis.StoreAll(suppliers);
                    redis.StoreAll(territories);
                }
            }
            else
            {
                Console.WriteLine("Unknown Provider: " + provider);
                Console.WriteLine("Available Providers:");
                Console.WriteLine(Providers.Join(", "));
            }
        }
    }
}
