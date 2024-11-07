using MySql.Data.MySqlClient;
using PFMS_MI04.Models;
using PFMS_MI04.Models.Authentication;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PFMS_MI04.Services
{
    public class ManageAccountService
    {
        //Changed to static
        private static readonly string connectionString = "Server=88.222.244.88;Port=3306;Database=account_schema;Uid=user;Pwd=mi04Curtin@2024#;";
        public async Task<List<ManageAccountUser>> GetUserDataAsync(string name, string role, string userId)
        {
            List<ManageAccountUser> users = new List<ManageAccountUser>();
            string query = "SELECT acc_name, user_id, acc_role FROM accounts ";

            // Append conditions based on the search criteria
            if (!string.IsNullOrEmpty(name))
            {
                query += " AND acc_name LIKE @name";
            }
            if (!string.IsNullOrEmpty(role))
            {
                query += " AND acc_role LIKE @role";
            }
            if (!string.IsNullOrEmpty(userId))
            {
                query += " AND user_id LIKE @userId";
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync(); 
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    // Added parameters to prevent SQL injection
                    if (!string.IsNullOrEmpty(name))
                    {
                        cmd.Parameters.AddWithValue("@name", $"%{name}%");
                    }
                    if (!string.IsNullOrEmpty(role))
                    {
                        cmd.Parameters.AddWithValue("@role", $"%{role}%");
                    }
                    if (!string.IsNullOrEmpty(userId))
                    {
                        cmd.Parameters.AddWithValue("@userId", $"%{userId}%");
                    }

                    using (MySqlDataReader reader = await cmd.ExecuteReaderAsync() as MySqlDataReader) 
                    {
                        while (await reader.ReadAsync()) //asynchronous reading
                        {
                            // Create user object
                            var user = new ManageAccountUser
                            {
                                AccName = reader["acc_name"].ToString(),
                                UserId = reader["user_id"].ToString(),
                                Role = reader["acc_role"].ToString(),
                                Status = AuthRepoUser.IsUserLoggedIn(reader["user_id"].ToString()) ? "Online" : "Offline"
                            };

                            users.Add(user);
                        }
                    }
                }
            }

            return users;
        }

        public async Task<bool> RemoveUserInDb(AccountModel request)
        {
            // Construct the delete query
            string query = "DELETE FROM accounts WHERE user_id = @userId"; 

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync(); 
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    // Added parameter to prevent SQL injection
                    cmd.Parameters.AddWithValue("@userId", request.UserID);

                    // Execute the command and return the number of affected rows
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0; // Return true if at least one row was deleted
                }
            }
        }
    }
}
