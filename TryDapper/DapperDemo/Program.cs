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
            //var order = GetOrder(10297); // simple query
            //var order = GetOrderDynamic(10297); // dynamic objects
            //var orderDetails = GetDetailsForOrder(10297); // multi mapping
            var allOrders = GetAllOrdersWithDetails(); // query multiple

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

        public static IEnumerable<OrderDetails> GetDetailsForOrder(int orderID)
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();

                string sql = @"
select od.*, '' as [SplitProduct], p.*, '' as [SplitSupplier], s.*, '' as [SplitCategory], c.* from [Order Details] od
join [Products] p on od.ProductID = p.ProductID
join [Suppliers] s on p.SupplierID = s.SupplierID
join [Categories] c on p.CategoryID = c.CategoryID
where od.OrderID = @OrderID 
";

                var orderDetails = connection.Query<OrderDetails, Product, Supplier, Category, OrderDetails>
                    (sql, (od, p, s, c) =>
                    {
                        p.Supplier = s;
                        p.Category = c;
                        od.Product = p;
                        return od;
                    },
                    new { OrderID = orderID },
                    splitOn: "SplitProduct, SplitSupplier, SplitCategory");

                return orderDetails;
            }
        }

        private static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["NorthwindDB"].ToString();
        }

        public static IEnumerable<Order> GetAllOrdersWithDetails()
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["NorthwindDB"].ToString()))
            {
                connection.Open();

                string sql = @"
SELECT * FROM Orders
SELECT * FROM [Order Details]
";
                using (var result = connection.QueryMultiple(sql))
                {
                    var ordersDict = result.Read<Order>().ToDictionary(o => o.OrderID, o => o);
                    var orderDetails = result.Read<OrderDetails>().ToList();

                    foreach (var orderDetail in orderDetails)
                    {
                        if (ordersDict.ContainsKey(orderDetail.OrderID))
                        {
                            if (ordersDict[orderDetail.OrderID].OrderDetails == null)
                            {
                                ordersDict[orderDetail.OrderID].OrderDetails = new List<OrderDetails>();
                            }

                            ordersDict[orderDetail.OrderID].OrderDetails.Add(orderDetail);
                        }
                    }

                    return ordersDict.Values;
                }
            }
        }
    }
}
