using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;

namespace KDSAPI.Data
{
    public class UsersDAO
    {
        private string connectionString = "Server=localhost;Port=3306;Database=kds;User Id=root;Password=root";


        public UserModel GetByID(int id)
        {
            UserModel user = null;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string query = "SELECT Id, Username, Password FROM Users WHERE Id = @Id";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    user = new UserModel
                    {
                        Id = (int)reader["Id"],
                        Username = (string)reader["Username"],
                        Password = (string)reader["Password"]
                    };
                }
            }
            return user;
        }

        public UserModel GetByUsername(string username)
        {
            UserModel user = null;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string query = "SELECT Id, Username, Password FROM Users WHERE Username = @Username";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", username);

                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    user = new UserModel
                    {
                        Id = (int)reader["Id"],
                        Username = (string)reader["Username"],
                        Password = (string)reader["Password"]
                    };
                }
            }
            return user;
        }

        internal IActionResult Create(string username, string hashedPassword)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string query = "INSERT INTO Users (Username, Password) VALUES (@Username, @Password)";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Password", hashedPassword);
                connection.Open();
                if (command.ExecuteNonQuery() > 0)
                {
                    return new OkResult();
                }
                else
                {
                    return new BadRequestResult();
                }
            }
        }
    }



    public class UserModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
