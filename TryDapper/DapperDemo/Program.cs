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
            var order = GetOrder(10297);

            Console.WriteLine();
        }

        public static Order GetOrder(int id)
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["NorthwindDB"].ToString()))
            {
                connection.Open();

                string sql = "SELECT * FROM Orders WHERE OrderID = @ID";

                var order = connection.Query<Order>(sql, new { ID = id }).FirstOrDefault();

                return order;
            }
        }
    }
}
