using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using DapperDemo.Models;

namespace DapperDemo
{
    class Program
    {
        static void Main()
        {
            //var order = GetOrder(10297);
            var order = GetOrderDynamic(10297);

            Console.WriteLine();
        }

        public static Order GetOrder(int id)
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();

                string sql = "SELECT * FROM Orders WHERE OrderID = @ID";

                var order = connection.Query<Order>(sql, new { ID = id }).FirstOrDefault();

                return order;
            }
        }

        public static Order GetOrderDynamic(int id)
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();

                string sql = "SELECT * FROM Orders WHERE OrderID = @ID";

                var dynamicObject = connection.Query(sql, new { ID = id }).FirstOrDefault();

                return new Order 
                {
                    CustomerID = dynamicObject.CustomerID,
                    EmployeeID = dynamicObject.EmployeeID,
                    Freight = dynamicObject.Freight,
                    OrderDate = dynamicObject.OrderDate,
                    OrderID = dynamicObject.OrderID,
                    RequiredDate = dynamicObject.RequiredDate,
                    ShipAddress = dynamicObject.ShipAddress,
                    ShipCity = dynamicObject.ShipCity,
                    ShipCountry = dynamicObject.ShipCountry,
                    ShipName = dynamicObject.ShipName,
                    ShippedDate = dynamicObject.ShippedDate,
                    ShipPostalCode = dynamicObject.ShipPostalCode,
                    ShipRegion = dynamicObject.ShipRegion,
                    ShipVia = dynamicObject.ShipVia
                }; 
            }
        }

        private static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["NorthwindDB"].ToString();
        }
    }
}
