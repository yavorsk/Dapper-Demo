using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data;
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
            // simple query - get by id
            Order order = GetOrder(10297);

            // simple query using dynamic mapping
            //var order = GetOrderDynamic(10297);

            // multi mapping
            //var orderDetails = GetDetailsForOrder(10297);

            // process multiple result grids (query multiple)
            //var allOrders = GetAllOrdersWithDetails();

            // execute
            //order.RequiredDate = order.RequiredDate.Value.AddDays(-1);
            //bool orderIsUpdated = UpdateOrder(order);

            // calling stored procedures
            Product newProduct = GetNewProduct();
            newProduct.ProductID = CreateProduct(newProduct);

            // transactions
            bool orderIsDeleted = DeleteOrder(10297);
        }

        private static bool DeleteOrder(int orderID)
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                 connection.Open();

                 using (var transaction =  connection.BeginTransaction())
                 {
                     try
                     {
                         const string sqlDeleteOrderDetails = @"DELETE FROM [Order Details] WHERE OrderID=@OrderID";
                         const string sqlDeleteOrder = @"DELETE FROM [Orders] WHERE OrderID=@OrderID";

                         int orderDetailsRowsAffected = connection.Execute(sqlDeleteOrderDetails, new { OrderID = orderID }, transaction);
                         int orderRowsAffected = connection.Execute(sqlDeleteOrderDetails, new { OrderID = orderID }, transaction);
                         throw new Exception();

                         transaction.Commit();

                         return orderRowsAffected > 0 ? true : false;
                     }
                     catch (Exception ex)
                     {
                         transaction.Rollback();
                         return false;
                     }
                 }
            }
        }

        private static int CreateProduct(Product newProduct)
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();

                // calling stored procedure using 'Query'
                var uspParams = new
                {
                    ProductName = newProduct.ProductName,
                    SupplierID = newProduct.SupplierID,
                    CategoryID = newProduct.CatrogryID,
                    QuantityPerUnit = newProduct.QuantityPerUnit,
                    UnitPrice = newProduct.UnitPrice,
                    UnitsInStock = newProduct.UnitsInStock,
                    UnitsOnOrder = newProduct.UnitsInOrder,
                    ReorderLevel = newProduct.ReorderLevel,
                    Discontinued = newProduct.Discontinued
                };

                int productID = connection.Query<int>("usp_CreateProduct2", uspParams, commandType: CommandType.StoredProcedure).FirstOrDefault();
                return productID;

                // calling stored procedure using 'Execute' and dynamic parameters
                //var uspParams = new DynamicParameters();
                //uspParams.Add("@ProductName", newProduct.ProductName);
                //uspParams.Add("@SupplierID", newProduct.SupplierID);
                //uspParams.Add("@CategoryID", newProduct.CatrogryID);
                //uspParams.Add("@QuantityPerUnit", newProduct.QuantityPerUnit);
                //uspParams.Add("@UnitPrice", newProduct.UnitPrice);
                //uspParams.Add("@UnitsInStock", newProduct.UnitsInStock);
                //uspParams.Add("@UnitsOnOrder", newProduct.UnitsInOrder);
                //uspParams.Add("@ReorderLevel", newProduct.ReorderLevel);
                //uspParams.Add("@Discontinued", newProduct.Discontinued);
                //uspParams.Add("@ProductID", dbType: DbType.Int32, direction: ParameterDirection.Output);

                //connection.Execute("usp_CreateProduct", uspParams, commandType: CommandType.StoredProcedure);

                //return uspParams.Get<int>("@ProductID");
            }
        }

        private static Product GetNewProduct()
        {
            return new Product
            {
                CatrogryID = 1,
                Discontinued = false,
                ProductName = "Beer",
                QuantityPerUnit = "15",
                ReorderLevel = null,
                SupplierID = 1,
                UnitPrice = 60,
                UnitsInOrder = 0,
                UnitsInStock = 250
            };
        }

        private static bool UpdateOrder(Order order)
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();

                string sql = @"
UPDATE Orders
SET OrderDate = @OrderDate,
    RequiredDate = @RequiredDate, 
    ShippedDate = @ShippedDate,
    ShipVia = @ShipVia,
    Freight = @Freight,
    ShipName = @ShipName,
    ShipAddress = @ShipName, 
    ShipCity = @ShipCity,
    ShipRegion = @ShipRegion,
    ShipPostalCode = @ShipPostalCode,
    ShipCountry = @ShipCountry
WHERE OrderID = @OrderID
";
                 int rowsAffected = connection.Execute(sql, order);

                 if (rowsAffected > 0)
                 {
                     return true;
                 }
                 else
                 {
                     return false;
                 }
            }
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
