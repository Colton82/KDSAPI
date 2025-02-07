using KDSAPI.Models;
using Mysqlx.Crud;
using Newtonsoft.Json;

namespace KDSAPI.Data
{
    public class OrderDAO
    {
        private string connectionString = "Server=localhost;Port=3306;Database=kds;User Id=root;Password=root";


        public void SaveOrder(string jsonOrder)
        {
            var order = JsonConvert.DeserializeObject<OrderModel>(jsonOrder);
            Console.WriteLine($"Saving order {order.Id} to database...");

            using (var connection = new MySql.Data.MySqlClient.MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO orders (Id, CustomerName, Timestamp, Users_id) VALUES (@Id, @CustomerName, @Timestamp, @Users_id)";

                using (var command = new MySql.Data.MySqlClient.MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", order.Id);
                    command.Parameters.AddWithValue("@CustomerName", order.CustomerName);
                    command.Parameters.AddWithValue("@Timestamp", order.Timestamp);
                    command.Parameters.AddWithValue("@Users_id", order.Users_id);
                    command.ExecuteNonQuery();
                }
            }

            Console.WriteLine($"Order {order.Id} saved to database.");
        }
    }
}
