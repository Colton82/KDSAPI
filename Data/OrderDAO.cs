using KDSAPI.Models;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static KDSAPI.Models.DynamicOrderModel;
using System.Data;

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
        public async Task SaveOrder(DynamicOrderModel order)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string query = "INSERT INTO orders (CustomerName, Timestamp, Users_id, Station, Items) " +
                                   "VALUES (@CustomerName, @Timestamp, @Users_id, @Station, @Items)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CustomerName", order.CustomerName);
                        command.Parameters.AddWithValue("@Timestamp", order.Timestamp); // Keep as string
                        command.Parameters.AddWithValue("@Users_id", order.Users_id);
                        command.Parameters.AddWithValue("@Station", order.Station ?? (object)DBNull.Value);

                        // Serialize List<OrderItem> to JSON
                        string itemsJson = JsonConvert.SerializeObject(order.Items);
                        command.Parameters.AddWithValue("@Items", itemsJson);

                        await command.ExecuteNonQueryAsync();
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
        /// Retrieves all orders for a specific user.
        /// </summary>
        public async Task<List<DynamicOrderModel>> GetOrdersByUserName(string username)
        {
            List<DynamicOrderModel> orders = new List<DynamicOrderModel>();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Get user ID from username
                    string query = "SELECT id FROM users WHERE username = @username";
                    int userId = -1;

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                userId = reader.GetInt32(0);
                            }
                        }
                    }

                    if (userId == -1)
                    {
                        Console.WriteLine($"No user found with username {username}.");
                        return orders;
                    }

                    // Get orders using userId
                    query = "SELECT Id, CustomerName, Timestamp, Station, Items FROM orders WHERE Users_Id = @userId";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", userId);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var order = new DynamicOrderModel
                                {
                                    Id = reader.GetInt32("Id"),
                                    CustomerName = reader.GetString("CustomerName"),
                                    Timestamp = reader.GetString("Timestamp"),
                                    Station = reader.IsDBNull(reader.GetOrdinal("Station")) ? null : reader.GetString("Station")
                                };

                                // Deserialize Items JSON into List<OrderItem>
                                string itemsJson = reader.IsDBNull(reader.GetOrdinal("Items")) ? "[]" : reader.GetString("Items");
                                order.Items = JsonConvert.DeserializeObject<List<OrderItem>>(itemsJson) ?? new List<OrderItem>();

                                orders.Add(order);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving orders: {ex.Message}");
            }

            return orders;
        }


        /// <summary>
        /// Retrieves an order based on its ID.
        /// </summary>
        public async Task<DynamicOrderModel> GetOrderById(int id)
        {
            DynamicOrderModel order = null;

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT Id, CustomerName, Timestamp, Items, Station FROM orders WHERE Id = @Id";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                order = new DynamicOrderModel
                                {
                                    Id = reader.GetInt32("Id"),
                                    CustomerName = reader.GetString("CustomerName"),
                                    Timestamp = reader.GetString("Timestamp"),
                                    Station = reader.IsDBNull(reader.GetOrdinal("Station")) ? null : reader.GetString("Station")
                                };

                                // Deserialize Items JSON into List<OrderItem>
                                string itemsJson = reader.IsDBNull(reader.GetOrdinal("Items")) ? "[]" : reader.GetString("Items");
                                order.Items = JsonConvert.DeserializeObject<List<OrderItem>>(itemsJson) ?? new List<OrderItem>();
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
        /// Updates an existing order.
        /// </summary>
        public async Task<bool> UpdateOrderAsync(DynamicOrderModel order)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string query = "UPDATE orders SET CustomerName = @CustomerName, Timestamp = @Timestamp, Users_id = @Users_id, Items = @Items, Station = @Station WHERE Id = @Id";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", order.Id);
                        command.Parameters.AddWithValue("@CustomerName", order.CustomerName);
                        command.Parameters.AddWithValue("@Timestamp", order.Timestamp);
                        command.Parameters.AddWithValue("@Users_id", order.Users_id);
                        command.Parameters.AddWithValue("@Station", order.Station ?? (object)DBNull.Value);

                        // Serialize Items
                        string itemsJson = JsonConvert.SerializeObject(order.Items);
                        command.Parameters.AddWithValue("@Items", itemsJson);

                        await command.ExecuteNonQueryAsync();
                    }
                }
                Console.WriteLine($"Order {order.Id} updated successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating order: {ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// Deletes an order by its ID.
        /// </summary>
        public async Task<bool> DeleteOrderAsync(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string query = "DELETE FROM orders WHERE Id = @Id";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        int rowsAffected = await command.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            Console.WriteLine($"Order {id} deleted successfully.");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine($"No order found with ID {id}.");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting order: {ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// Archives a completed order by moving it to the `past_orders` table.
        /// </summary>
        public async Task<bool> ArchiveOrderAsync(DynamicOrderModel order)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string query = "INSERT INTO past_orders (Id, CustomerName, Timestamp, Users_id, Station, Items) " +
                                   "VALUES (@Id, @CustomerName, @Timestamp, @Users_id, @Station, @Items)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", order.Id);
                        command.Parameters.AddWithValue("@CustomerName", order.CustomerName);
                        command.Parameters.AddWithValue("@Timestamp", order.Timestamp);
                        command.Parameters.AddWithValue("@Users_id", order.Users_id);
                        command.Parameters.AddWithValue("@Station", order.Station ?? (object)DBNull.Value);

                        string itemsJson = JsonConvert.SerializeObject(order.Items);
                        command.Parameters.AddWithValue("@Items", itemsJson);

                        int rowsAffected = await command.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            Console.WriteLine($"Order {order.Id} archived successfully.");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine($"Failed to archive order {order.Id}.");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error archiving order: {ex.Message}");
            }
            return false;
        }

        public async Task<AnalyticsResponse> GetAnalyticsAsync(DateTime startDate, string username)
        {
            List<DynamicOrderModel> orders = new List<DynamicOrderModel>();

            try
            {
                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();
                string query = "SELECT id FROM users WHERE username = @username";
                int userId = -1;

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            userId = reader.GetInt32(0);
                        }
                    }
                }

                if (userId == -1)
                {
                    Console.WriteLine($"No user found with username '{username}'.");
                    return null;
                }

                query = @"SELECT * FROM past_orders WHERE STR_TO_DATE(RIGHT(Timestamp, LOCATE('|', REVERSE(Timestamp)) - 2), '%Y-%m-%dT%H:%i:%s.%f') >= @StartDate AND users_id = @Id;";

                using var orderCommand = new MySqlCommand(query, connection);
                orderCommand.Parameters.AddWithValue("@StartDate", startDate.ToString("yyyy-MM-dd HH:mm:ss"));
                orderCommand.Parameters.AddWithValue("@Id", userId);

                Console.WriteLine($"Executing query:\n{query}");
                Console.WriteLine($"Parameters: @StartDate = '{startDate:yyyy-MM-dd HH:mm:ss}', @Id = {userId}");

                using var orderReader = await orderCommand.ExecuteReaderAsync();
                while (await orderReader.ReadAsync())
                {
                    orders.Add(new DynamicOrderModel
                    {
                        Id = orderReader.GetInt32("id"),
                        CustomerName = orderReader.GetString("customerName"),
                        Station = orderReader.GetString("Station"),
                        Timestamp = orderReader.GetString("Timestamp"),
                        Items = JsonConvert.DeserializeObject<List<DynamicOrderModel.OrderItem>>(orderReader.GetString("Items"))
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving analytics: {ex.Message}");
                return null;
            }

            return CalculatePerformanceMetrics(orders);
        }




        private static AnalyticsResponse CalculatePerformanceMetrics(List<DynamicOrderModel> orders)
        {
            var stationTimes = new Dictionary<string, List<double>>();
            var ticketTimes = new List<double>();
            var dayCounts = new Dictionary<string, int>();  // To track busiest days
            var hourCounts = new Dictionary<int, int>();   // To track peak hours

            foreach (var order in orders)
            {
                var timestamps = ExtractStationTimestamps(order.Timestamp);

                if (timestamps.Count > 1)
                {
                    // Total order time
                    var orderTime = (timestamps.Last().Value - timestamps.First().Value).TotalMinutes;
                    ticketTimes.Add(orderTime);

                    // Compute time spent at each station
                    var stationList = timestamps.Keys.ToList();
                    for (int i = 0; i < stationList.Count - 1; i++)
                    {
                        string station = stationList[i];
                        double timeSpent = (timestamps[stationList[i + 1]] - timestamps[station]).TotalMinutes;

                        if (!stationTimes.ContainsKey(station))
                        {
                            stationTimes[station] = new List<double>();
                        }

                        stationTimes[station].Add(timeSpent);
                    }

                    // Track busiest days and peak hours
                    DateTime firstTimestamp = timestamps.First().Value;
                    string dayOfWeek = firstTimestamp.DayOfWeek.ToString();
                    int hourOfDay = firstTimestamp.Hour;

                    if (dayCounts.ContainsKey(dayOfWeek))
                        dayCounts[dayOfWeek]++;
                    else
                        dayCounts[dayOfWeek] = 1;

                    if (hourCounts.ContainsKey(hourOfDay))
                        hourCounts[hourOfDay]++;
                    else
                        hourCounts[hourOfDay] = 1;
                }
            }

            // Compute overall averages
            double avgTicketTime = ticketTimes.Any() ? ticketTimes.Average() : 0;

            var stationStats = stationTimes
                .Select(station => new StationPerformance
                {
                    Station = station.Key,
                    Percentage = Math.Round((station.Value.Sum() / ticketTimes.Sum()) * 100, 2),
                    AvgTime = station.Value.Any() ? station.Value.Average() : 0
                })
                .OrderByDescending(s => s.Percentage)
                .ToList();

            // Determine busiest days and peak hours
            var busiestDays = dayCounts.OrderByDescending(d => d.Value)
                                       .Take(3)
                                       .Select(d => new BusiestDay { Day = d.Key, OrderCount = d.Value })
                                       .ToList();

            var peakHours = hourCounts.OrderByDescending(h => h.Value)
                                      .Take(3)
                                      .Select(h => new PeakHour { Hour = h.Key, OrderCount = h.Value })
                                      .ToList();

            return new AnalyticsResponse
            {
                AverageTicketTime = avgTicketTime,
                StationPerformance = stationStats,
                BusiestDays = busiestDays,
                PeakHours = peakHours
            };
        }



        private static Dictionary<string, DateTime> ExtractStationTimestamps(string timestampData)
        {
            var stationTimestamps = new Dictionary<string, DateTime>();

            if (string.IsNullOrWhiteSpace(timestampData)) return stationTimestamps;

            var parts = timestampData.Split('|');

            for (int i = 0; i < parts.Length - 1; i += 2)
            {
                string station = parts[i].Trim();
                if (DateTime.TryParse(parts[i + 1].Trim(), out DateTime parsedDate))
                {
                    stationTimestamps[station] = parsedDate;
                }
            }

            return stationTimestamps;
        }


        public class AnalyticsResponse
        {
            public double AverageTicketTime { get; set; }
            public List<StationPerformance> StationPerformance { get; set; }
            public List<BusiestDay> BusiestDays { get; set; }
            public List<PeakHour> PeakHours { get; set; }
        }

        public class BusiestDay
        {
            public string Day { get; set; }
            public int OrderCount { get; set; }
        }

        public class PeakHour
        {
            public int Hour { get; set; }
            public string FormattedHour => $"{Hour}:00 - {Hour}:59";
            public int OrderCount { get; set; }
        }


        public class StationPerformance
        {
            public string Station { get; set; }
            public double Percentage { get; set; }
            public double AvgTime { get; set; }
        }

    }
}
