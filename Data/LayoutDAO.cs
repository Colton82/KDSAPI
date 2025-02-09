using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace KDSAPI.Data
{
    /// <summary>
    /// Data access object for layout data.
    /// </summary>
    public class LayoutDAO
    {
        private string connectionString = "Server=localhost;Port=3306;Database=kds;User Id=root;Password=root";

        /// <summary>
        /// Retrieves layout string, splits it into an array of strings and returns
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string[] GetLayoutByUserId(int id)
        {
            string[] stations = null;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string query = "SELECT LayoutData FROM Layouts WHERE UserId = @Id";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);
                connection.Open();

                System.Diagnostics.Debug.WriteLine($"Querying LayoutData for UserId: {id}");

                var result = command.ExecuteScalar();
                if (result == null)
                {
                    System.Diagnostics.Debug.WriteLine($"No layout data found for UserId: {id}");
                    return null;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"LayoutData: {result.ToString()}");
                }

                stations = result.ToString().Split(",");

            }
            return stations;
        }

        /// <summary>
        /// Saves the layout for the specified user.
        /// </summary>
        /// <param name="id">The user ID.</param>
        /// <param name="layout">The layout data as a string.</param>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
        public bool SaveLayout(int id, string layout)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Try to update existing layout
                    string updateQuery = "UPDATE Layouts SET LayoutData = @layout WHERE UserId = @id";
                    using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@layout", layout);
                        updateCommand.Parameters.AddWithValue("@id", id);
                        int rowsAffected = updateCommand.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return true;
                        }
                    }

                    // If none exists, create new layout record
                    string insertQuery = "INSERT INTO Layouts (UserId, LayoutData) VALUES (@id, @layout)";
                    using (MySqlCommand insertCommand = new MySqlCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@id", id);
                        insertCommand.Parameters.AddWithValue("@layout", layout);
                        int rowsAffected = insertCommand.ExecuteNonQuery();

                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving layout: {ex.Message}");
                return false;
            }
        }
    }
}
