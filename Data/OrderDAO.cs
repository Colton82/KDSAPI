using KDSAPI.Models;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace KDSAPI.Data
{
    /// <summary>
    /// Data Access Object for orders.
    /// </summary>
    public class OrderDAO
    {
        private string connectionString = "Server=localhost;Port=3306;Database=kds;User Id=root;Password=root";

        /// <summary>
        /// Saves the order to the database, including Items as JSON.
        /// </summary>
        public void SaveOrder(string jsonOrder)
        {
            Console.WriteLine($"Received JSON: {jsonOrder}");

            try
            {
                var rawOrder = JsonConvert.DeserializeObject<JObject>(jsonOrder);

                // Extract Items and serialize it manually
                var itemsToken = rawOrder["Items"];
                Console.WriteLine($"Extracted ItemsToken: {itemsToken}");
                string itemsJsonString = itemsToken != null ? itemsToken.ToString(Formatting.None) : null;
                Console.WriteLine($"Serialized ItemsJson: {itemsJsonString}");

                // Remove "Items" from rawOrder before converting it to OrderModel
                rawOrder.Remove("Items");

                // Deserialize the rest of the object
                var order = rawOrder.ToObject<OrderModel>();

                // Assign ItemsJson manually after deserialization
                order.ItemsJson = itemsJsonString;

                Console.WriteLine($"Final ItemsJson: {order.ItemsJson}");

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO orders (Id, CustomerName, Timestamp, Users_id, Station, Items) " +
                                   "VALUES (@Id, @CustomerName, @Timestamp, @Users_id, @Station, @Items)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", order.Id);
                        command.Parameters.AddWithValue("@CustomerName", order.CustomerName);
                        command.Parameters.AddWithValue("@Timestamp", order.Timestamp);
                        command.Parameters.AddWithValue("@Users_id", order.Users_id);
                        command.Parameters.AddWithValue("@Station", order.Station ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Items", order.ItemsJson ?? (object)DBNull.Value);
                        Console.WriteLine($"Final ItemsJson (Before Saving): {order.ItemsJson}");

                        command.ExecuteNonQuery();
                    }
                }

                Console.WriteLine($"Order {order.Id} saved to database.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving order: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves orders for a specific user and converts Items back to a Dictionary.
        /// </summary>
        public OrderModel[] GetOrdersByUserName(string username)
        {
            List<OrderModel> orders = new List<OrderModel>();
            int userId;

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    //-------Get userID from username------------//
                    string query = "SELECT id FROM users WHERE username = @username";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        using (var reader = command.ExecuteReader())
                        {
                            reader.Read();
                            userId = reader.GetInt32(0);
                        }
                    }




                    //------Get orders using userId------------//
                    query = "SELECT Id, CustomerName, Timestamp, Users_id, Station, Items FROM orders WHERE Users_Id = @userId";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", userId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Console.WriteLine($"Id: {reader["Id"]}");
                                Console.WriteLine($"CustomerName: {reader["CustomerName"]}");
                                Console.WriteLine($"Timestamp: {reader["Timestamp"]}");
                                Console.WriteLine($"Users_id: {reader["Users_id"]}");
                                Console.WriteLine($"Station: {reader["Station"]}");
                                Console.WriteLine($"Items: {reader["Items"]}");

                                orders.Add(new OrderModel
                                {
                                    Id = reader.IsDBNull(reader.GetOrdinal("Id")) ? 0 : reader.GetInt32("Id"),
                                    CustomerName = reader.IsDBNull(reader.GetOrdinal("CustomerName")) ? "Unknown" : reader.GetString("CustomerName"),
                                    Timestamp = reader.IsDBNull(reader.GetOrdinal("Timestamp")) ? "Unknown" : reader.GetString("Timestamp"),
                                    Users_id = reader.IsDBNull(reader.GetOrdinal("Users_id")) ? 0 : reader.GetInt32("Users_id"),
                                    Station = reader.IsDBNull(reader.GetOrdinal("Station")) ? null : reader.GetString("Station"),
                                    ItemsJson = reader.IsDBNull(reader.GetOrdinal("Items")) ? "{}" : reader.GetString("Items")
                                });
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving orders: {ex.Message}");
            }

            return orders.ToArray();
        }

        /// <summary>
        /// Retrieves an order based on its ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public OrderModel GetOrderById(int id)
        {
            OrderModel order = null;

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    Console.WriteLine($"Connected to database. Running query for ID: {id}");

                    string query = "SELECT Id, CustomerName, Timestamp, Users_id, Items, Station FROM orders WHERE Id = @Id";
                    Console.WriteLine($"Executing Query: {query} with Id = {id}");

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read()) // If a row is found
                            {
                                order = new OrderModel
                                {
                                    Id = reader.GetInt32("Id"),
                                    CustomerName = reader.IsDBNull(reader.GetOrdinal("CustomerName")) ? "Unknown" : reader.GetString("CustomerName"),
                                    Timestamp = reader.IsDBNull(reader.GetOrdinal("Timestamp")) ? "Unknown" : reader.GetString("Timestamp"),
                                    Users_id = reader.IsDBNull(reader.GetOrdinal("Users_id")) ? -1 : reader.GetInt32("Users_id"),
                                    ItemsJson = reader.IsDBNull(reader.GetOrdinal("Items")) ? "{}" : reader.GetString("Items"),
                                    Station = reader.IsDBNull(reader.GetOrdinal("Station")) ? "Unknown" : reader.GetString("Station")
                                };
                                Console.WriteLine($"Order found: {order.Id}, {order.CustomerName}");
                            }
                            else
                            {
                                Console.WriteLine($"No order found with ID {id}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving order with ID {id}: {ex.Message}");
            }

            return order;
        }

        /// <summary>
        /// Updates an order based on its ID
        /// </summary>
        /// <param name="existingOrder"></param>
        /// <returns></returns>
        public async Task<bool> UpdateOrderAsync(OrderModel existingOrder)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string query = "UPDATE orders SET CustomerName = @CustomerName, Timestamp = @Timestamp, Users_id = @Users_id, Items = @Items, Station = @Station WHERE Id = @Id";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", existingOrder.Id);
                        command.Parameters.AddWithValue("@CustomerName", existingOrder.CustomerName);
                        command.Parameters.AddWithValue("@Timestamp", existingOrder.Timestamp);
                        command.Parameters.AddWithValue("@Users_id", existingOrder.Users_id);
                        command.Parameters.AddWithValue("@Items", existingOrder.ItemsJson);
                        command.Parameters.AddWithValue("@Station", existingOrder.Station);

                        await command.ExecuteNonQueryAsync();
                    }
                }
                Console.WriteLine($"Order {existingOrder.Id} updated successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating order: {ex.Message}");
            }
            return false;
        }
    }
}
