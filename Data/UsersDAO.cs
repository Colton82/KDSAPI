using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;

namespace KDSAPI.Data
{
    /// <summary>
    /// Data access object for user data.
    /// </summary>
    public class UsersDAO : IUsersDAO
    {
        private string connectionString = "Server=canyon-kds.mysql.database.azure.com;Port=3306;Database=kds;User Id=coltoncuellar;Password=Lolak82!";

        /// <summary>
        /// Retrieves a user by ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Retrieves a user by username, handling database connection failures.
        /// </summary>
        /// <param name="username">The username of the user to retrieve.</param>
        /// <returns>A UserModel if found; otherwise, null.</returns>
        public UserModel GetByUsername(string username)
        {
            UserModel user = null;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string query = "SELECT Id, Username, Password FROM Users WHERE Username = @Username";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    try
                    {
                        connection.Open();
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user = new UserModel
                                {
                                    Id = reader.GetInt32("Id"),
                                    Username = reader.GetString("Username"),
                                    Password = reader.GetString("Password")
                                };
                            }
                        }
                    }
                    catch (MySqlException ex)
                    {
                        Console.WriteLine($"Database connection error: {ex.Message}");
                        return null;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error retrieving user: {ex.Message}");
                        return null;
                    }
                }
            }

            return user;
        }


        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="hashedPassword"></param>
        /// <returns></returns>
        public IActionResult Create(string username, string hashedPassword)
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

        internal int GetIdByUseraname(string username)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                {
                    connection.Open();
                    string query = "SELECT Id FROM Users WHERE Username = @Username";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Username", username);
                    var result = command.ExecuteScalar();
                    if (result == null)
                    {
                        return -1;
                    }
                    else
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error retrieving user ID: {ex.Message}");
                return -1;
            }
        }
    }


    /// <summary>
    /// Model for user data.
    /// </summary>
    public class UserModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
